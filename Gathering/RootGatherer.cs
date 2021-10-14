using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static BudgetReview.Gathering.MacuGatherer;
using static BudgetReview.Gathering.AmazonGatherer;
using static BudgetReview.Gathering.CitiCardGatherer;
using static BudgetReview.Gathering.ChaseCardGatherer;
using System.Collections.Generic;

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
            gatherers.Add(new MacuGatherer());
            gatherers.Add(new AmazonGatherer());
        }

        private async Task<DataSet<RawDataItem>> GatherAll()
        {
            var result = new DataSet<RawDataItem>();

            var tasks = new List<Task>();
            foreach (var g in gatherers)
            {
                tasks.Add(g.GatherInto(result));
            }
            Task.WaitAll(tasks.ToArray());

            AddCitiCardTransactions(result);
            AddChaseCardTransactions(result);

            await (await BrowserAutomationGatherer.GetInstance()).DisposeAsync();
            return result;
        }
    }
}
