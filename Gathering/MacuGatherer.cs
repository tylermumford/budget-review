using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Serilog;
using BudgetReview.Gathering.Dates;

namespace BudgetReview.Gathering
{
    internal class MacuGatherer : IGatherer
    {
        public async Task GatherInto(DataSet<RawDataGroup> results)
        {
            var filename = await DownloadAsync();
            var fileLoader = new FileLoader(results, Directory.GetCurrentDirectory());
            fileLoader.AddFile(Source.MACU, filename);
        }

        ///<summary>Creates a browser to download MACU transactions</summary>
        ///<returns>Filename of downloaded CSV file</returns>
        private async Task<string> DownloadAsync()
        {
            Log.Information("Downloading MACU transactions...");
            var url = Env.GetOrThrow("macu_sign_in_url");
            var username = Env.GetOrThrow("macu_username");
            var password = Env.GetOrThrow("macu_password");
            var account = Env.GetOrThrow("macu_account_id");

            var automation = await BrowserAutomationSingleton.SharedInstance;
            var page = await automation.CreatePageAsync();

            // Log in
            await page.GotoAsync(url);
            // await page.ClickAsync("[name=username]");
            await page.FillAsync("#username", username);
            await page.FillAsync("#password", password);
            await page.RunAndWaitForNavigationAsync(async () =>
                await page.ClickAsync("button[type=submit]")
            , new PageRunAndWaitForNavigationOptions
            {
                UrlString = "https://o.macu.com/DashboardV2",
                WaitUntil = WaitUntilState.NetworkIdle,
            });

            // Open the "Download Transactions" slider for the main account
            Log.Debug("MACU: Opening the export form");
            await page.ClickAsync($"[href*=account-{account}]");
            await page.ClickAsync("#export_trigger");
            var format = await page.WaitForSelectorAsync("#export-format-dropdown");
            if (format == null)
                throw new Exception("Couldn't get the format dropdown");

            // Fill out the form
            Log.Debug("MACU: Filling out the export form");
            await format.ClickAsync();
            await page.ClickAsync(".iris-list-item[data-value=\"54\"]");

            await page.FillAsync("#Parameters_StartDate", DateRange.FirstDay.ToIso());
            await page.FillAsync("#Parameters_EndDate", DateRange.LastDay.ToIso());

            // Get the exported CSV file
            Log.Debug("MACU: Downloading");
            var download = await page.RunAndWaitForDownloadAsync(async () =>
                await page.ClickAsync("#export_transactions_confirm_button")
            );
            var filename = $"Macu-{Guid.NewGuid()}.csv";
            await download.SaveAsAsync(filename);

            await page.CloseAsync();
            Log.Information("MACU: Finished downloading");
            return filename;
        }
    }
}
