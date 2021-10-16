using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Playwright;
using Serilog;

namespace BudgetReview.Gathering
{
    internal class BrowserAutomationGatherer : IAsyncDisposable
    {
        private static BrowserAutomationGatherer? instance;
        private static SemaphoreSlim instanceGuard = new(1, 1);

        private IPlaywright playwright;

        private IBrowser browser;

        private BrowserAutomationGatherer(IPlaywright p, IBrowser b)
        {
            playwright = p;
            browser = b;
        }

        public static bool HasInstance { get => instance != null; }

        public static async Task<BrowserAutomationGatherer> GetInstance()
        {
            if (instance != null)
                return instance;

            try
            {
                await instanceGuard.WaitAsync();

                if (instance != null)
                    return instance;

                Log.Information("Creating brand new BrowserAutomationGatherer");

                var p = await Playwright.CreateAsync();
                var b = await p.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = Convert.ToBoolean(Env.Get("headless", "true")),
                    SlowMo = Convert.ToInt32(Env.Get("slow_mo_delay", "0")),
                });

                instance = new BrowserAutomationGatherer(p, b);
                return instance;
            }
            finally
            {
                instanceGuard.Release();
            }
        }

        public async Task<IPage> CreatePageAsync()
        {
            var c = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                AcceptDownloads = true,
            });
            var p = await c.NewPageAsync();
            return p;
        }

        public async ValueTask DisposeAsync()
        {
            Log.Debug("Disposing BrowserAutomationGatherer");
            await instanceGuard.WaitAsync();
            Log.Verbose("Got instanceGuard");
            await browser.DisposeAsync();
            Log.Verbose("Disposed browser object");
            playwright.Dispose();
            Log.Verbose("Disposed playwright object");
            instance = null;
            instanceGuard.Release();
            Log.Verbose("Released instanceGuard");
        }
    }
}
