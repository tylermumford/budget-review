using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Playwright;
using System.Diagnostics;

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

        public static async Task<BrowserAutomationGatherer> GetInstance()
        {
            if (instance != null)
                return instance;

            await instanceGuard.WaitAsync();

            if (instance != null)
                return instance;

            Debug.WriteLine("Creating brand new BrowserAutomationGatherer");

            try
            {
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
            Debug.WriteLine("Disposing BrowserAutomationGatherer");
            await instanceGuard.WaitAsync();
            await browser.DisposeAsync();
            playwright.Dispose();
            instance = null;
            instanceGuard.Release();
        }
    }
}
