namespace BudgetReview.Gathering
{
    internal static class CitiCardGatherer
    {
        public static void AddCitiCardTransactions(DataSet<RawDataItem> results)
        {
            var g = new FileGatherer(results, Constants.RawDataDir);
            // Example: From 8_1_2021 To 8_31_2021.CSV
            g.AddFile(Source.CitiCard, @"From \d\d?_\d\d?_\d{4} To \d\d?_\d\d?_\d{4}\.CSV");
        }
    }
}
