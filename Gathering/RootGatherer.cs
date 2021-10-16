using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;

namespace BudgetReview.Gathering
{
    internal class RootGatherer
    {
        private List<IGatherer> gatherers = new();

        public Task<DataSet<RawDataItem>> Start()
        {
            InstantiateGatherers();
            return GatherAll();
        }

        private void InstantiateGatherers()
        {
            Log.Verbose("Instantiating gatherers");
            gatherers.Add(new MacuGatherer());
            gatherers.Add(new AmazonGatherer());
            gatherers.Add(new CitiCardGatherer());
            gatherers.Add(new ChaseCardGatherer());
        }

        private async Task<DataSet<RawDataItem>> GatherAll()
        {
            var result = new DataSet<RawDataItem>();

            var tasks = new List<Task>();
            foreach (var g in gatherers)
            {
                // Note: This is not thread-safe, because DataSet doesn't synchronize its
                // underlying List. Add synchronization or have GatherInto return results
                // that get merged on one thread.
                tasks.Add(g.GatherInto(result));
            }
            Task.WaitAll(tasks.ToArray());

            if (BrowserAutomationGatherer.HasInstance)
                await (await BrowserAutomationGatherer.GetInstance()).DisposeAsync();
            return result;
        }
    }
}
