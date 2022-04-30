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
            Log.Information("Downloading Amazon transactions...");

            var username = Env.GetOrThrow("amazon_username");
            var password = Env.GetOrThrow("amazon_password");

            var automation = await BrowserAutomationSingleton.SharedInstance;
            var page = await automation.CreatePageAsync();

            await page.GotoAsync(Env.GetOrThrow("amazon_sign_in_url"));

            // Sign in
            // await page.ClickAsync("#nav-link-accountList");
            await page.FillAsync("#ap_email", username);
            await page.ClickAsync("#continue");
            await page.FillAsync("#ap_password", password);
            await page.RunAndWaitForNavigationAsync(async () =>
                await page.ClickAsync("#signInSubmit")
            );

            // Fill out order reports form
            Log.Debug("Amazon: Filling out the report form");
            await page.GotoAsync("https://www.amazon.com/gp/b2b/reports?ref_=ya_d_l_order_reports");
            await Task.WhenAll(
                page.SelectOptionAsync("#report-month-start", FirstDay.Month.ToString()),
                page.SelectOptionAsync("#report-day-start", FirstDay.Day.ToString()),
                page.SelectOptionAsync("#report-year-start", FirstDay.Year.ToString()),
                page.SelectOptionAsync("#report-month-end", LastDay.Month.ToString()),
                page.SelectOptionAsync("#report-day-end", LastDay.Day.ToString()),
                page.SelectOptionAsync("#report-year-end", LastDay.Year.ToString())
            );

            // Download the report CSV file
            Log.Debug("Amazon: Downloading");
            const int timeout = 45_000;
            var download = await page.RunAndWaitForDownloadAsync
            (
                async () => await page.ClickAsync("#report-confirm"),
                new PageRunAndWaitForDownloadOptions { Timeout = timeout }
            );
            var filename = $"Amazon-{Guid.NewGuid()}.csv";
            await download.SaveAsAsync(filename);

            await page.CloseAsync();
            Log.Information("Finished downloading Amazon transactions into {Filename}", filename);
            return filename;
        }
    }
}
