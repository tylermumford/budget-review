using System;
using Serilog;

namespace BudgetReview.Gathering.Dates
{
    public static class DateRange
    {
        public static DateTime FirstDay;
        public static DateTime LastDay;

        static DateRange()
        {
            var monthOffset = -1;
            if (Args.Contains("--current-month"))
                monthOffset = 0;

            Log.Verbose("Month offset is {MonthOffset}", monthOffset);

            var f = DateTime.Today.AddMonths(monthOffset);
            f = f.AddDays(-f.Day + 1);
            FirstDay = f;
            Log.Verbose("First day is {Date}", FirstDay);

            var l = DateTime.Today.AddMonths(monthOffset);
            l = l.AddDays(DateTime.DaysInMonth(l.Year, l.Month) - l.Day);
            LastDay = l;
            Log.Verbose("Last day is {Date}", LastDay);
        }
    }
}
