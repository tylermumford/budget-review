namespace BudgetReview.Analyzing
{
    internal interface IAnalyzer
    {
        AnalysisResult Analyze(DataSet<ParsedDataItem> parsedData);
    }
}
