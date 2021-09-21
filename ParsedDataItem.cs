using System.Collections.Generic;
using System.Linq;

namespace BudgetReview
{
    internal class ParsedDataItem
    {
        public Source Source { get; set; }
        public IEnumerable<ILineItem> LineItems { get; set; } = new List<ILineItem>();

        public override string ToString()
        {
            return $"Data parsed from {Source} with {LineItems.Count()} lines";
        }
    }
}
