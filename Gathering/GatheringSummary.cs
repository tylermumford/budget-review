using Serilog.Events;

namespace BudgetReview.Gathering
{
    internal record GatheringSummary(string Message, LogEventLevel Level = LogEventLevel.Information)
    {
    }
}
