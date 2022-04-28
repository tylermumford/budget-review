using System;
using System.Globalization;
using CsvHelper.Configuration;

namespace BudgetReview.Analyzing
{
    /// <summary>Describes how to convert a <see cref="Transaction"/> into a CSV record.</summary>
    internal class TransactionMap : ClassMap<Transaction>
    {
        public TransactionMap()
        {
            AutoMap(CultureInfo.CurrentCulture);
            Map(t => t.Date).Convert(t => t.Value.Date.ToShortDateString());
            Map(t => t.Amount).Convert(t => $"{t.Value.Amount:c}");
            Map(t => t.Category).Name("Category");
        }
    }
}
