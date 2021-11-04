using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Playwright;
using Serilog;

namespace BudgetReview.Gathering
{
    internal class BrowserAutomationGatherer : IAsyncDisposable
    {
        private IPlaywright playwright;

        private IBrowser browser;

        private BrowserAutomationGatherer(IPlaywright p, IBrowser b)
        {
            playwright = p;
            browser = b;
        }

        public static bool HasInstance { get => LazyInstance.IsValueCreated; }

        public static async Task<BrowserAutomationGatherer> GetInstanceUnlocked()
        {
            Log.Information("Creating new BrowserAutomationGatherer");

            var p = await Playwright.CreateAsync();
            var b = await p.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Convert.ToBoolean(Env.Get("headless", "true")),
                SlowMo = Convert.ToInt32(Env.Get("slow_mo_delay", "0")),
            });

            return new BrowserAutomationGatherer(p, b);
        }

        public static AsyncLazy<BrowserAutomationGatherer> LazyInstance = new (GetInstanceUnlocked);

        public async Task<IPage> CreatePageAsync()
        {
            var c = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                AcceptDownloads = true,
            });
            var timeout = Convert.ToInt32(Env.Get("default_timeout_ms", "30000"));
            Log.Debug("Default timeout: {Timeout}", timeout);
            c.SetDefaultTimeout(timeout);
            c.SetDefaultNavigationTimeout(timeout);

            var p = await c.NewPageAsync();
            return p;
        }

        public async ValueTask DisposeAsync()
        {
            Log.Debug("Disposing BrowserAutomationGatherer");
            await browser.DisposeAsync();
            Log.Verbose("Disposed browser object");
            playwright.Dispose();
            Log.Verbose("Disposed playwright object");
        }
    }
}
