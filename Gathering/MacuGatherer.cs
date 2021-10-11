using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace BudgetReview.Gathering
{
    internal static class MacuGatherer
    {
        public static async Task AddMacuTransactionsAsync(DataSet<RawDataItem> results)
        {
            /* var g = new FileGatherer(results, Constants.RawDataDir);
            // Example: ExportedTransactions-2.csv
            g.AddFile(Source.MACU, @"ExportedTransactions(-\d+)?.csv"); */

            var g = new InternetGatherer();
            await g.AddToAsync(results);
        }

        private class InternetGatherer
        {
            internal async Task AddToAsync(DataSet<RawDataItem> results)
            {
                var username = Env.GetOrThrow("macu_username");
                var password = Env.GetOrThrow("macu_password");
                var account = Env.GetOrThrow("macu_account_id");

                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false,
                    SlowMo = 800,
                });
                var page = await browser.NewPageAsync(new BrowserNewPageOptions
                {
                    AcceptDownloads = true,
                });

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
                await download.SaveAsAsync(download.SuggestedFilename);

                // Wait for a human to see the success
                await page.WaitForTimeoutAsync(1200);
            }
        }
    }
}
