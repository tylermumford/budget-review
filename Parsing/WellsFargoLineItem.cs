using System;
using CsvHelper.Configuration.Attributes;

namespace BudgetReview.Parsing
{
    internal record WellsFargoLineItem : ILineItem
    {
        [Index(0)]
        public DateTime Date { get; set; }

        [Index(1)]
        public decimal Amount { get; set; }

        [Index(4)]
        public string Description { get; set; } = "";
    }
}
