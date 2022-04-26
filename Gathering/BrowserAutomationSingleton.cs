using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Playwright;
using Serilog;
using System.Diagnostics;

namespace BudgetReview.Gathering
{
    ///<summary>Manages a pool of browser automation objects.</summary>
    internal class BrowserAutomationSingleton
    {
        public static AsyncLazy<BrowserAutomation> SharedInstance = new (CreateLazyInstance);

        private static async Task<BrowserAutomation> CreateLazyInstance()
        {
            return await BrowserAutomation.CreateAsync();
        }
    }
}
