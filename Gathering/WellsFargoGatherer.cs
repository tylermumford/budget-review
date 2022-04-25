using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Serilog;
using BudgetReview.Gathering.Dates;

namespace BudgetReview.Gathering
{
    internal class WellsFargoGatherer : IGatherer
    {
        public async Task GatherInto(DataSet<RawDataItem> results)
        {
            var filename = await DownloadAsync();
            var fileLoader = new FileLoader(results, Directory.GetCurrentDirectory());
            fileLoader.AddFile(Source.WellsFargo, filename);
        }

        ///<summary>Creates a browser to download Wells Fargo transactions</summary>
        ///<returns>Filename of downloaded CSV file</returns>
        private async Task<string> DownloadAsync()
        {
            Log.Information("Downloading Wells Fargo transactions...");
            var url = Env.GetOrThrow("wells_fargo_sign_in_url");
            var username = Env.GetOrThrow("wells_fargo_username");
            var password = Env.GetOrThrow("wells_fargo_password");

            var automation = await BrowserAutomationPool.LazyInstance;
            var page = await automation.CreatePageAsync();

            // Log in
            await page.GotoAsync(url);
            await page.FillAsync("#j_username", username);
            await page.FillAsync("#j_password", password);
            await page.RunAndWaitForNavigationAsync(async () =>
                await page.ClickAsync("button:text(\"Sign on\")")
            , new PageRunAndWaitForNavigationOptions
            {
                UrlString = "https://connect.secure.wellsfargo.com/accounts/start",
                WaitUntil = WaitUntilState.NetworkIdle,
            });

            // Click the link for the card account (as opposed to the rewards account)
            Log.Debug("MACU: Opening the export form");
            await page.ClickAsync($"link:text(\"Card\")");

            if (await page.IsVisibleAsync("text=Help us make sure you're a person"))
            {
                // CAPTCHA detected
            }

            // await page.ClickAsync("#export_trigger");
            // var format = await page.WaitForSelectorAsync("#export-format-dropdown");
            // if (format == null)
            //     throw new Exception("Couldn't get the format dropdown");

            // // Fill out the form
            // Log.Debug("MACU: Filling out the export form");
            // await format.ClickAsync();
            // await page.ClickAsync(".iris-list-item[data-value=\"54\"]");

            // await page.FillAsync("#Parameters_StartDate", DateRange.FirstDay.ToIso());
            // await page.FillAsync("#Parameters_EndDate", DateRange.LastDay.ToIso());

            // // Get the exported CSV file
            // Log.Debug("MACU: Downloading");
            // var download = await page.RunAndWaitForDownloadAsync(async () =>
            //     await page.ClickAsync("#export_transactions_confirm_button")
            // );
            // var filename = $"Macu-{Guid.NewGuid()}.csv";
            // await download.SaveAsAsync(filename);

            // await page.CloseAsync();
            // Log.Information("MACU: Finished downloading");
            // return filename;
            return "NotQuiteImplementedYet";
        }
    }
}
