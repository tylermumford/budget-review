using System.Collections.Generic;

namespace BudgetReview.Extensions
{
    public static class StringExtensions
    {
        ///<summary>Returns true if the string contains any of the given targets.</summary>
        public static bool ContainsAny(this string subject, IEnumerable<string> targets)
        {
            foreach (var target in targets)
            {
                if (subject.Contains(target))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
