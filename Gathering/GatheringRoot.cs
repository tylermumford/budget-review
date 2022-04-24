using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;
using BudgetReview.Gathering.Dates;
using static BudgetReview.Gathering.Dates.DateRange;
using System;

namespace BudgetReview.Gathering
{
    ///<summary>Starts all the code that gathers records of money spent.</summary>
    internal class GatheringRoot
    {
        private List<IGatherer> gatherers = new();

        public Task<DataSet<RawDataItem>> Start()
        {
            Console.WriteLine($"Gathering transactions between {FirstDay.ToIso()} and {LastDay.ToIso()}");
            InstantiateGatherers();
            return GatherAll();
        }

        private void InstantiateGatherers()
        {
            Log.Verbose("Instantiating gatherers");
            gatherers.Add(new AmazonGatherer());
            gatherers.Add(new MacuGatherer());
            gatherers.Add(new CitiCardGatherer());
            gatherers.Add(new ChaseCardGatherer());
            gatherers.Add(new ChaseAmazonGatherer());
        }

        private async Task<DataSet<RawDataItem>> GatherAll()
        {
            var result = new DataSet<RawDataItem>();

            var tasks = new List<Task>();
            var max = Convert.ToInt32(Env.Get("max_simultaneous_gatherers", "2"));
            foreach (var g in gatherers)
            {
                // Note: This is not thread-safe, because DataSet doesn't synchronize its
                // underlying List. Add synchronization or have GatherInto return results
                // that get merged on one thread.

                if (tasks.Count < max)
                {
                    tasks.Add(g.GatherInto(result));
                }
                else
                {
                    await Task.WhenAny(tasks);
                    tasks.RemoveAll(t => t.IsCompleted);
                    tasks.Add(g.GatherInto(result));
                }
            }
            await Task.WhenAll(tasks);

            if (BrowserAutomationPool.HasInstance)
                await (await BrowserAutomationPool.LazyInstance).DisposeAsync();
            return result;
        }
    }
}
