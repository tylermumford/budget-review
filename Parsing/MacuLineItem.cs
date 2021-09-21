using System;
using CsvHelper.Configuration.Attributes;

namespace BudgetReview.Parsing
{
    internal record MacuLineItem : ILineItem
    {
        [Name("Effective Date")]
        public DateTime Date { get; set; }

        [Name("Amount")]
        public decimal Amount { get; set; }

        [Name("Description")]
        public string Description { get; set; } = "";
    }
}
