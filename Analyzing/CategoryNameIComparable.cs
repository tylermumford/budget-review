using System;

namespace BudgetReview.Analyzing
{
    // CategoryName objects are comparable among themselves and among string values.

    internal partial class CategoryName : IComparable<CategoryName>, IComparable<string>
    {
        public int CompareTo(CategoryName? other) => Name.CompareTo(other?.Name);

        public int CompareTo(string? other) => Name.CompareTo(other);
    }
}
