using System;
using System.Globalization;
using CsvHelper.Configuration.Attributes;

namespace BudgetReview.Parsing
{
    internal record CitiCardLineItem : ILineItem
    {
        [Name("Date")]
        public DateTime Date { get; set; }

        [Name("Debit")]
        [Default(0)]
        [NumberStyles(NumberStyles.Currency)]
        public decimal Amount { get; set; }

        [Default(0)]
        [Obsolete]
        /// <summary>
        /// Alias for Amount, for use in CSV parsing. Not to be used directly.
        /// </summary>
        public decimal Credit { set { if (Amount == 0) Amount = value; } }

        [Name("Description")]
        public string Description { get; set; } = "";
    }
}