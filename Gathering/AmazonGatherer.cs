using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Serilog;
using static BudgetReview.Gathering.Dates.DateRange;

namespace BudgetReview.Gathering
{
    internal class AmazonGatherer : IGatherer
    {
        private ILogger Logger = new PrefixedLogger(Log.Logger, "Amazon");

        public async Task GatherInto(DataSet<RawDataGroup> results)
        {
            var filename = await DownloadAsync();
            var fileLoader = new FileLoader(results, Directory.GetCurrentDirectory());
            fileLoader.AddFile(Source.Amazon, filename);
        }

        // There used to be a method here for retrying after failure, but it
        // wasn't being used, so it wasn't needed.

        private async Task<string> DownloadAsync()
        {
            Logger.Information("Downloading transactions...");

            var username = Env.GetOrThrow("amazon_username");
            var password = Env.GetOrThrow("amazon_password");

            var automation = await BrowserAutomationSingleton.SharedInstance;
            var page = await automation.CreatePageAsync();

            Logger.Debug("Loading sign in page");
            await page.GotoAsync(Env.GetOrThrow("amazon_sign_in_url"),
                new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            Logger.Debug("Filling out sign in form");
            await page.FillAsync("#ap_email", username);
            await page.ClickAsync("#continue");
            await page.FillAsync("#ap_password", password);

            Logger.Debug("Submitting sign in form and waiting for DOMContentLoaded");
            await page.RunAndWaitForNavigationAsync(async () =>
                await page.ClickAsync("#signInSubmit")
            , new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            // Fill out order reports form
            Logger.Debug("Navigating to report form and waiting for DOMContentLoaded");
            await page.GotoAsync("https://www.amazon.com/gp/b2b/reports",
                new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

            Logger.Debug("Filling out report form");
            await page.FillAsync("#startDateCalendar input", FirstDay.ToShortDateString());
            await page.FillAsync("#endDateCalendar input", LastDay.ToShortDateString());

            // Download the report CSV file
            Logger.Debug("Downloading");
            const int timeout = 45_000;
            var download = await page.RunAndWaitForDownloadAsync
            (
                async () => await page.ClickAsync("#report-confirm"),
                new PageRunAndWaitForDownloadOptions { Timeout = timeout }
            );
            var filename = $"Amazon-{Guid.NewGuid()}.csv";
            await download.SaveAsAsync(filename);

            await page.CloseAsync();
            Logger.Information("Finished downloading transactions into {Filename}", filename);
            return filename;
        }
    }
}
