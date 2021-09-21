namespace BudgetReview.Gathering
{
    internal static class AmazonGatherer
    {
        public static void AddAmazonTransactions(DataSet<RawDataItem> results, string dir)
        {
            var g = new FileGatherer(results, dir);
            // Example: 01-Aug-2021_to_01-Sep-2021.csv
            g.AddFile(Source.Amazon, @"\d{2}-\w+-\d{4}_to_\d{2}-\w+-\d{4}\.csv");
        }
    }
}
