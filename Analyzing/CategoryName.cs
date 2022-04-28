using System.Collections.Generic;

namespace BudgetReview.Analyzing
{
    ///<summary>
    /// Represensts a transaction category. Created by string inputs, but managed
    /// so that identical strings always produce the same Category instance.
    ///</summary>
    internal partial class CategoryName
    {
        /// <summary>
        /// Gets the category with the given name, creating it if necessary.
        /// </summary>
        public static CategoryName Of(string name)
        {
            if (!Instances.ContainsKey(name))
                Instances[name] = new CategoryName(name);

            return Instances[name];
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// A pre-existing Category that represends the lack of a well-named category.
        /// </summary>
        public static CategoryName Uncategorized { get; } = new("Uncategorized");

        private static readonly Dictionary<string, CategoryName> Instances = new();

        private CategoryName(string name)
        {
            Name = name;
        }

        private readonly string Name;
    }
}
