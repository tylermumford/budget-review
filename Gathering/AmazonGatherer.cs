using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BudgetReview.Gathering
{
    internal class AmazonGatherer : IGatherer
    {
        public async Task GatherInto(DataSet<RawDataItem> results)
        {
            // var g = new FileGatherer(results, Constants.RawDataDir);
            // // Example: 01-Aug-2021_to_01-Sep-2021.csv
            // g.AddFile(Source.Amazon, @"\d{2}-\w+-\d{4}_to_\d{2}-\w+-\d{4}\.csv");
            var filename = await DownloadAsync();
            // var fileLoader
            return;
        }

        private async Task<string> DownloadAsync()
        {
            Debug.WriteLine("Downloading Amazon transactions...");

            var username = Env.GetOrThrow("amazon_username");
            var password = Env.GetOrThrow("amazon_password");

            var automation = await BrowserAutomationGatherer.GetInstance();
            var page = await automation.CreatePageAsync();

            await page.GotoAsync("https://amazon.com");

            // Sign in
            await page.ClickAsync("#nav-link-accountList");
            await page.FillAsync("#ap_email", username);
            await page.ClickAsync("#continue");
            await page.FillAsync("#ap_password", password);
            await page.ClickAsync("#signInSubmit");

            // Fill out order reports form
            await page.GotoAsync("https://www.amazon.com/gp/b2b/reports?ref_=ya_d_l_order_reports");
            await page.EvaluateAsync("setDatesToLastMonth()");
            await page.ClickAsync("#report-confirm");

            // TODO: Figure out how to properly wait for the download

            await page.CloseAsync();
            return "none";
        }
    }
}
