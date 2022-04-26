using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Serilog;
using static BudgetReview.Gathering.Dates.DateRange;

namespace BudgetReview.Gathering
{
    internal class CitiCardGatherer : IGatherer
    {
        public async Task GatherInto(DataSet<RawDataItem> results)
        {
            var filename = await DownloadAsync();
            var fileLoder = new FileLoader(results, Directory.GetCurrentDirectory());
            fileLoder.AddFile(Source.CitiCard, filename);
        }

        private async Task<string> DownloadAsync()
        {
            Log.Information("Downloading Citi card transactions...");
            var url = Env.GetOrThrow("citi_sign_in_url");
            var username = Env.GetOrThrow("citi_username");
            var password = Env.GetOrThrow("citi_password");

            var automation = await BrowserAutomationSingleton.SharedInstance;
            var page = await automation.CreatePageAsync();

            // Log in
            await page.GotoAsync(url);
            Log.Debug("Waiting for NetworkIdle...");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.FillAsync("#username", username);
            await page.FillAsync("#password", password);
            await page.RunAndWaitForNavigationAsync(async () =>
            {
                await page.ClickAsync("#signInBtn");
                Log.Debug("Clicked sign in button");

                // Deal with possible promotional page
                if (await page.IsVisibleAsync("text=Earn a $100-$300 Cash Bonus"))
                {
                    Log.Information("Dealing with promotional page.");
                    await page.ClickAsync("text=No Thanks");
                }
                else
                {
                    Log.Debug("No promotional page detected.");
                }
            }
            , new PageRunAndWaitForNavigationOptions
            {
                UrlString = "https://online.citi.com/US/ag/mrc/dashboard",
            });

            // Deal with possibility of multiple accounts
            var activeCardSelector = "text=Card-8979";
            if (await page.IsVisibleAsync(activeCardSelector))
            {
                Log.Information("Clicking into active card account.");
                await page.ClickAsync(activeCardSelector);
            }
            else
            {
                Log.Debug("No need to click into active card account.");
            }

            // Fill out the form
            Log.Debug("Citi: Filling out form");
            var options = new PageFillOptions();
            await page.ClickAsync("#timePeriodFilterDropdown");
            await page.ClickAsync("text=Date range");
            await page.FillAsync("#fromDatePicker", $"{FirstDay:MM\\/dd\\/yyyy}", options);
            await page.FillAsync("#toDatePicker", $"{LastDay:MM\\/dd\\/yyyy}", options);

            Log.Debug("Citi: Clicking buttons to start download");
            await page.ClickAsync("#dateRangeApplyButton");
            await page.ClickAsync("#exportTransactionsLink");
            await page.ClickAsync("text=CSV"); // it's the default, but just to be sure

            Log.Debug("Citi: Downloading");
            var download = await page.RunAndWaitForDownloadAsync(async () =>
                await page.ClickAsync("#exportModal button:text-is(\"Export\")")
            );
            var filename = $"Citi-{Guid.NewGuid()}.csv";
            await download.SaveAsAsync(filename);

            await page.CloseAsync();
            Log.Information("Citi: Finished downloading");
            return filename;
        }
    }
}
