using System;

namespace BudgetReview
{
    public interface ILineItem
    {
        DateTime Date { get; set; }
        decimal Amount { get; set; }
        string Description { get; set; }
    }
}
