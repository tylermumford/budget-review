using System;

namespace BudgetReview.Gathering.Dates
{
    public static class DateTimeExtensions
    {
        public static string ToIso(this DateTime d) => $"{d:yyyy-MM-dd}";
    }
}
