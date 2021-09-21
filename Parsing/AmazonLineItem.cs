using System;
using System.Globalization;
using CsvHelper.Configuration.Attributes;

namespace BudgetReview.Parsing
{
    internal record AmazonLineItem : ILineItem
    {
        [Name("Order Date")]
        public DateTime Date { get; set; }

        [Name("Item Total")]
        [NumberStyles(NumberStyles.Currency)]
        public decimal Amount { get; set; }

        [Name("Title")]
        public string Description { get; set; } = "";
    }
}