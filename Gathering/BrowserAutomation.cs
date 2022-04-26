using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Serilog;

namespace BudgetReview.Gathering
{
    internal class BrowserAutomation
    {
        public static async Task<BrowserAutomation> CreateAsync()
        {
            var instance = new BrowserAutomation();
            await instance.InitializeAsync();
            return instance;
        }

        public async Task<IPage> CreatePageAsync()
        {
            var clock = new Stopwatch();
            clock.Start();

            if (browser is null)
                throw new InvalidOperationException($"Cannot call {nameof(CreatePageAsync)} when {nameof(browser)} is not initialized.");

            var c = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                AcceptDownloads = true,
            });
            var timeout = Convert.ToInt32(Env.Get("default_timeout_ms", "30000"));
            Log.Debug("Default timeout: {Timeout}", timeout);
            c.SetDefaultTimeout(timeout);
            c.SetDefaultNavigationTimeout(timeout);

            var p = await c.NewPageAsync();

            Log.Information("CreatePageAsync took {Duration}ms", clock.ElapsedMilliseconds);
            return p;
        }

        private IBrowser? browser;

        private BrowserAutomation() { }

        private async Task InitializeAsync()
        {
            Log.Information("Initializing Playwright & Firefox");

            var playwright = await Playwright.CreateAsync();
            browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Convert.ToBoolean(Env.Get("headless", "true")),
                SlowMo = Convert.ToInt32(Env.Get("slow_mo_delay", "0")),
            });
        }
    }
}
