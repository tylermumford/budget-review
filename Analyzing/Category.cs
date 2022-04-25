using System;
using System.Collections.Generic;

namespace BudgetReview.Analyzing
{
    ///<summary>
    /// Represensts a transaction category. Created by string inputs, but managed
    /// so that identical strings always produce the same Category instance.
    ///</summary>
    internal class Category
    {
        public string Name { get; private set; }

        public override string ToString()
        {
            return Name ?? "<unset category>";
        }

        public static Category Uncategorized { get; } = new("Uncategorized");

        /// <summary>
        /// Gets or creates the named category.
        /// </summary>
        public static Category ByName(string name)
        {
            if (!set.ContainsKey(name))
                set[name] = new Category(name);

            return set[name];
        }

        public static IEnumerable<Category> GetAll() => set.Values;

        private static readonly Dictionary<string, Category> set = new();

        private Category(string name)
        {
            Name = name;
        }
    }
}
