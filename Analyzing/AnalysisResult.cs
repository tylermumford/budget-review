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
            var output = new StringBuilder();

            foreach (var t in SortedTransactions())
                output.AppendLine(t.ToString());

            output.AppendLine();

            foreach (var c in CategoryTotals())
                output.AppendLine($"{c.Category,-14}: {c.Total:C}");

            // TODO: Sum non-income transactions (all expenses)

            return output.ToString();
        }

        private IOrderedEnumerable<Transaction> SortedTransactions()
        {
            return Transactions
                .OrderBy(t => t.Category.Name)
                .ThenByDescending(t => Math.Abs(t.Amount));
        }

        private IOrderedEnumerable<CategoryTotal> CategoryTotals()
        {
            return Transactions
                .GroupBy(t => t.Category)
                .Select(g => new CategoryTotal(g.Key, g.Sum(t => t.Amount)))
                .OrderByDescending(x => Math.Abs(x.Total));
        }
    }
}
