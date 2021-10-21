using static System.Environment;

namespace BudgetReview
{
    internal static class Args
    {
        public static bool Contains(string flag) => CommandLine.Contains(flag);
    }
}
