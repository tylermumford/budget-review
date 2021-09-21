namespace BudgetReview.Gathering
{
    internal static class MacuGatherer
    {
        public static void AddMacuTransactions(DataSet<RawDataItem> results)
        {
            var g = new FileGatherer(results, Constants.RawDataDir);
            // Example: ExportedTransactions-2.csv
            g.AddFile(Source.MACU, @"ExportedTransactions(-\d+)?.csv");
        }
    }
}
