using System;
using System.Globalization;
using CsvHelper.Configuration.Attributes;

namespace BudgetReview.Parsing
{
    internal record ChaseCardLineItem : ILineItem
    {
        [Name("Transaction Date")]
        public DateTime Date { get; set; }

        [Name("Amount")]
        [NumberStyles(NumberStyles.Currency)]
        public decimal Amount { get; set; }

        [Name("Description")]
        public string Description { get; set; } = "";
    }
}
