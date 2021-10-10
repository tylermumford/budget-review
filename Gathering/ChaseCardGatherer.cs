namespace BudgetReview.Gathering
{
    internal static class ChaseCardGatherer
    {
        public static void AddChaseCardTransactions(DataSet<RawDataItem> results)
        {
            var g = new FileGatherer(results, Constants.RawDataDir);
            // Example: Chase7878_Activity20210801_20210831_20210922.CSV
            g.AddFile(Source.ChaseCard, @"Chase7878_.+\.CSV");
        }
    }
}
