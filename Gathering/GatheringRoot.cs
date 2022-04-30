using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;
using BudgetReview.Gathering.Dates;
using static BudgetReview.Gathering.Dates.DateRange;
using System;
using System.Linq;
using Serilog.Events;

namespace BudgetReview.Gathering
{
    ///<summary>Starts all the code that gathers records of money spent.</summary>
    internal class GatheringRoot : IGatheringRoot
    {
        private List<IGatherer> gatherers = new();

        private DataSet<RawDataGroup> result = new();

        public GatheringSummary Summary { get; private set; } = new("Haven't gathered yet.");

        public Task<DataSet<RawDataGroup>> Start()
        {
            Console.WriteLine($"Gathering transactions between {FirstDay.ToIso()} and {LastDay.ToIso()}");
            InstantiateGatherers();
            return GatherAll();
        }

        private void InstantiateGatherers()
        {
            Log.Verbose("Instantiating gatherers");
            gatherers.Add(new CitiCardGatherer()); // First, since it currently seems less reliable
            gatherers.Add(new AmazonGatherer());
            gatherers.Add(new MacuGatherer());
            gatherers.Add(new WellsFargoGatherer());
            gatherers.Add(new ChaseCardGatherer());
            gatherers.Add(new ChaseAmazonGatherer());
        }

        private async Task<DataSet<RawDataGroup>> GatherAll()
        {
            result = new DataSet<RawDataGroup>();

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

            SetSummary();

            return result;
        }

        private void SetSummary()
        {
            var attempted = gatherers.Count;
            var gotResults = result.Count();
            var level = (attempted == gotResults) ? LogEventLevel.Information : LogEventLevel.Error;

            Summary = new($"Tried to gather from {attempted} sources, and got results from {gotResults}", level);
        }
    }
}
