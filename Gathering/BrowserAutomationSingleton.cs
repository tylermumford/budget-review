using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Playwright;
using Serilog;
using System.Diagnostics;

namespace BudgetReview.Gathering
{
    ///<summary>Manages a pool of browser automation objects.</summary>
    internal class BrowserAutomationSingleton : IAsyncDisposable
    {
        private IPlaywright playwright;

        private IBrowser browser;

        private BrowserAutomationSingleton(IPlaywright p, IBrowser b)
        {
            playwright = p;
            browser = b;
        }

        public static bool HasInstance { get => LazyInstance.IsValueCreated; }

        public static async Task<BrowserAutomationSingleton> GetInstanceUnlocked()
        {
            Log.Information("Creating new BrowserAutomationPool");

            var p = await Playwright.CreateAsync();
            var b = await p.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Convert.ToBoolean(Env.Get("headless", "true")),
                SlowMo = Convert.ToInt32(Env.Get("slow_mo_delay", "0")),
            });

            return new BrowserAutomationSingleton(p, b);
        }

        public static AsyncLazy<BrowserAutomationSingleton> LazyInstance = new (GetInstanceUnlocked);

        public async Task<IPage> CreatePageAsync()
        {
            var clock = new Stopwatch();
            clock.Start();

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

        public async ValueTask DisposeAsync()
        {
            Log.Debug("Disposing BrowserAutomationPool");
            await browser.DisposeAsync();
            Log.Verbose("Disposed browser object");
            playwright.Dispose();
            Log.Verbose("Disposed playwright object");
        }
    }
}
