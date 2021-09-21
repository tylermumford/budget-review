using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BudgetReview.Analyzing
{
    internal record AnalysisResult
    {
        public List<Transaction> Transactions { get; set; } = new();

        public string GetDisplayString()
        {
            var sb = new StringBuilder();

            var sorted = Transactions
                .OrderBy(t => t.Category.Name)
                .ThenByDescending(t => Math.Abs(t.Amount));

            foreach (var t in sorted)
                sb.AppendLine(t.ToString());

            sb.AppendLine();

            var categories = Transactions
                .GroupBy(t => t.Category)
                .Select(g => new { g.Key.Name, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => Math.Abs(x.Total));
            foreach (var c in categories)
                sb.AppendLine($"{c.Name,-14}: {c.Total:C}");

            // TODO: Sum non-income transactions (all expenses)

            return sb.ToString();
        }
    }
}