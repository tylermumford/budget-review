using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace BudgetReview.Gathering
{
    internal class MacuGatherer : IGatherer
    {
        public async Task GatherInto(DataSet<RawDataItem> results)
        {
            if (Env.Get("skip_macu").ToLower() == "true")
            {
                Console.WriteLine("Configured to skip MacuGatherer");
                return;
            }

            var filename = await DownloadAsync();
            var fileLoader = new FileGatherer(results, Directory.GetCurrentDirectory());
            fileLoader.AddFile(Source.MACU, filename);
        }

        ///<summary>Creates a browser to download MACU transactions</summary>
        ///<returns>Filename of downloaded CSV file</returns>
        private async Task<string> DownloadAsync()
        {

            Debug.WriteLine("Downloading MACU transactions...");
            var username = Env.GetOrThrow("macu_username");
            var password = Env.GetOrThrow("macu_password");
            var account = Env.GetOrThrow("macu_account_id");

            var automation = await BrowserAutomationGatherer.GetInstance();
            var page = await automation.CreatePageAsync();

            // Log in
            await page.GotoAsync("https://www.macu.com/");
            await page.ClickAsync("[name=username]");
            await page.FillAsync("[name=username]", username);
            await page.FillAsync("[name=password]", password);
            await page.RunAndWaitForNavigationAsync(async () =>
                await page.ClickAsync("button[id*=login]")
            , new PageRunAndWaitForNavigationOptions
            {
                UrlString = "https://o.macu.com/DashboardV2",
            });

            // Open the "Download Transactions" slider for the main account
            await page.ClickAsync($"[href*=account-{account}]");
            await page.ClickAsync("#export_trigger");
            var format = await page.WaitForSelectorAsync("#export-format-dropdown");
            if (format == null)
                throw new Exception("Couldn't get the format dropdown");

            // Fill out the form
            await format.ClickAsync();
            await page.ClickAsync(".iris-list-item[data-value=\"54\"]");

            await page.FillAsync("#Parameters_StartDate", "2021-10-01");
            await page.FillAsync("#Parameters_EndDate", "2021-10-13");

            // Get the exported CSV file
            var download = await page.RunAndWaitForDownloadAsync(async () =>
                await page.ClickAsync("#export_transactions_confirm_button")
            );
            var filename = $"Macu-{Guid.NewGuid()}.csv";
            await download.SaveAsAsync(filename);

            await page.CloseAsync();
            Debug.WriteLine("Finished downloading MACU transactions");
            return filename;
        }
    }
}
