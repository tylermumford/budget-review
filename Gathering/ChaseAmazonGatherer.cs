using System.Threading.Tasks;

namespace BudgetReview.Gathering
{
    internal class ChaseAmazonGatherer : IGatherer
    {
        public Task GatherInto(DataSet<RawDataGroup> results)
        {
            var g = new FileLoader(results, Env.GetOrThrow("DOWNLOADS_DIRECTORY"));
            // Example: Chase4277_Activity20210801_20210831_20210922.CSV
            g.AddFile(Source.AmazonVisa, @"Chase4277_.+\.CSV");
            return Task.CompletedTask;
        }
    }
}
