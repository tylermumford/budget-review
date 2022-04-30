namespace BudgetReview.Parsing
{
    internal interface IParser
    {
        DataSet<ParsedDataItem> Parse(DataSet<RawDataItem> rawData);
    }
}
