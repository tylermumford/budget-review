using System;

namespace BudgetReview
{
    internal class RawDataGroup
    {
        public Source Source { get; set; }
        public string[] ContentLines { get; set; } = Array.Empty<string>();

        public override string ToString()
        {
            return $"Data from {Source} with {ContentLines.Length} lines";
        }
    }
}
