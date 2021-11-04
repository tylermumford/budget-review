using System;
using System.Collections.Generic;

namespace BudgetReview.Analyzing
{
    internal static class BasicAnalyzer
    {
        public static void Step(AnalysisResult result)
        {
            var ideas = new List<BasicIdea>
            {
                new("The Church Of Jesus Christ", "Tithing"),
                new("Bear River Mutual", "Insurance", "Bear River Mutual"),

                new("Pacificorp", "Utilities", "Rocky Mountain Power"),
                new("Questar", "Utilities", "Questar Gas"),
                new("TAYLORSVILLE-BENNION", "Utilities", "Water & sewer district"),
                new("WASATCH FRONT WASTE", "Utilities", "Garbage service"),

                new("COMCAST CABLE", "Internet/Cell", "Comcast internet"),
                new("XFINITY MOBILE", "Internet/Cell", "Xfinity Mobile"),

                new("DISNEYPLUS", "Entertainment", "Disney+"),
                new("Disney PLUS", "Entertainment", "Disney+"),
                new("Amazon Music", "Entertainment", "Amazon Musc"),
                new("APPLE Music", "Entertainment", "Amazon Musc"),
                new("Kindle Unltd", "Entertainment", "Kindle Unlimited"),

                new("BRIGHTBOX", "Gifts"),
                new("UNITEDWHOLESALE", "Housing", "Mortgage payment"),
                new("KID TO KID", "Kids", "Kid To Kid"),

                new("WALMART GROCERY", "Groceries", "Walmart Grocery"),
                new("WM SUPERCENTER", "Groceries", "Walmart (Store)"),
                new("COSTCO WHSE", "Groceries"),
                new("MACEY'S", "Groceries", "Macey's"),

                new("ZUPAS", "Eating Out", "Café Zupas"),
                new("MOBETTAHS", "Eating Out", "Mobettahs"),
                new("OLIVE GARDEN", "Eating Out", "Olive Garden"),
                new("MCDONALD", "Eating Out", "McDonald's"),
                new("WENDYS", "Eating Out", "Wendy's"),
                new("ARBYS", "Eating Out", "Arby's"),

                new("NOOM", "Tyler Fun", "Noom"),
                new("APPLE.COM", "Tyler Fun", "Apple"),
                new("EOS FITNESS", "Tyler Fun", "Eos Training"),
                new("40012 EOS", "Tyler Fun", "Eos Fitness"),
                new("DIGITALOCEAN", "Tyler Fun", "DigitalOcean servers"),

                new("CUROLOGY", "Sarah Fun", "Curology"),

                new("MONSTER 3", "Cars", "Car wash"),
                new("COSTCO GAS", "Cars", "Costco Gas"),

                new("HOME DEPOT", "Extras", "Home Depot"),
            };

            foreach (var idea in ideas)
            {
                foreach (var t in result.Transactions)
                {
                    const StringComparison mode = StringComparison.CurrentCultureIgnoreCase;
                    if (t.Description.Contains(idea.SearchPattern, mode))
                        idea.ApplyTo(t);
                }
            }
        }

        private record BasicIdea(
            string SearchPattern,
            string CategoryName,
            string? NewDescription = null)
        {
            public void ApplyTo(Transaction t)
            {
                t.Category = Category.ByName(CategoryName);

                if (NewDescription != null)
                    t.Description = NewDescription;
            }
        }
    }
}
