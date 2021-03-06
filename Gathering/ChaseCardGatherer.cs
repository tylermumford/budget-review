using System.Threading.Tasks;

namespace BudgetReview.Gathering
{
    internal class ChaseCardGatherer : IGatherer
    {
        public Task GatherInto(DataSet<RawDataGroup> results)
        {
            var g = new FileLoader(results, Env.GetOrThrow("DOWNLOADS_DIRECTORY"));
            // Example: Chase7878_Activity20210801_20210831_20210922.CSV
            g.AddFile(Source.ChaseCard, @"Chase7878_.+\.CSV");
            return Task.CompletedTask;
        }
    }
}
