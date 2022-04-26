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

            var automation = await BrowserAutomationSingleton.SharedInstance;
            var page = await automation.CreatePageAsync();

            // Log in
            await page.GotoAsync(url);
            await EnterUsernameAndPassword(page, username, password);
            await page.ClickAsync("button:text(\"Sign on\")");


            // Barrier #1: CAPTCHA
            // -------------------

            if (await page.IsVisibleAsync("text=Help us make sure you're a person"))
            {
                // CAPTCHA detected
                Log.Warning("CAPTCHA detected! A person will need to help with this.");
                await EnterUsernameAndPassword(page, username, password);
                await page.FocusAsync("#nucaptcha-answer");

                await page.WaitForNavigationAsync();
            }
            else
            {
                Log.Debug("No CAPTCHA detected.");
            }

            // Barrier #2: Offer for checking account
            // --------------------------------------

            if (await page.IsVisibleAsync("text=Simplify your life with Everyday Checking"))
            {
                Log.Warning("Offer for a checking account detected! Declining.");
                await page.ClickAsync("button:text(\"No Thanks\")");
            }
            else
            {
                Log.Debug("No promotional offer detected.");
            }

            // Okay, back on track.
            // --------------------

            // Click the link for the card account (as opposed to the rewards account)
            await page.ClickAsync("[class*=AccountTitle]");

            await page.ClickAsync("text=Download Account Activity");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.ClickAsync("text=Comma Delimited");
            // (This should happen before the dates are entered, otherwise their "Force=true" adjustment
            // will cause the gatherer to try to fill them in before they're available.)

            // NOTE: When I've done this manually, there's sometimes a modal with the text "No Internet connection."
            // But I haven't seen this in any automated runs.

            Log.Information($"Using first date like {DateRange.FirstDay.ToShortDateString()}");
            var allowInvisible = new PageFillOptions { Force = true };
            await WaitForDateFieldsToBeActionable(page);
            await page.FillAsync("#fromDate", DateRange.FirstDay.ToShortDateString(), allowInvisible);
            await page.FillAsync("#toDate", DateRange.LastDay.ToShortDateString(), allowInvisible);

            var download = await page.RunAndWaitForDownloadAsync(async () =>
            {
                Log.Information("Clicking the Download button");
                await page.ClickAsync("button:text('Download')");
            });
            var filename = $"WellsFargo-{Guid.NewGuid()}.csv";
            await download.SaveAsAsync(filename);
            Log.Information("Wells Fargo: Finished downloading");

            await page.CloseAsync();
            return filename;
        }

        // It sucks that a method like this seems required. Sheesh.
        private async Task WaitForDateFieldsToBeActionable(IPage page)
        {
            // Wait for proxy element to be clickable
            Log.Debug("summoning calendar popup");
            await page.ClickAsync(".mwf-select-date-proxy");

            // Determine what month name to look for
            var preselectedDate = await page.InputValueAsync("#fromDate");
            var date = DateTime.Parse(preselectedDate);
            var month = date.ToString("MMMM");
            var selector = $".mwf-calendar-month-label:text('{month}')";
            Log.Debug("will wait for selector: " + selector);

            // Wait for that month name (in the calendar popup) to appear
            await page.WaitForSelectorAsync(selector);
            Log.Debug("popup has appeared, dismissing...");

            // Dismiss the calendar popup
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden });
            Log.Debug("popup is gone");
        }

        private async Task EnterUsernameAndPassword(IPage page, string username, string password)
        {
            await page.FillAsync("#j_username", username);
            await page.FillAsync("#j_password", password);
        }
    }
}
