﻿using System;
using System.Linq;

namespace BudgetReview.Analyzing
{
    internal record Transaction : ILineItem
    {
        private readonly ILineItem line;
        private Category category = Category.Uncategorized;

        public Transaction(ILineItem line, Source source = Source.Undefined)
        {
            this.line = line;
            Source = source;
        }

        public DateTime Date { get => line.Date; set => line.Date = value; }
        public decimal Amount { get => line.Amount; set => line.Amount = value; }
        public string Description { get => line.Description; set => line.Description = value; }

        public Source Source { get; set; }
        public Category Category
        {
            get => category;
            set
            {
                if (category != Category.Uncategorized)
                    Console.WriteLine("!! Category conflict !!");

                category = value;
            }
        }

        public override string ToString()
        {
            var cutDescription = string.Join("", Description.Take(52));
            return $"• {Category.Name,-14} {Amount,-10:C} on {Date:MMM dd} for {cutDescription}";
        }
    }
}