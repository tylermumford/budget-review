using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BudgetReview
{
    ///<summary>Holds a generic, add- and get-only collection of items.</summary>
    internal class DataSet<T> : IEnumerable<T>
    {
        private readonly List<T> items = new();

        public void Add(T data) => items.Add(data);

        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append($"(DataSet<{typeof(T)} {items.Count} items)");
            foreach (var item in items)
            {
                b.Append('\t');
                b.Append(item?.ToString() ?? "<null>");
            }
            return b.ToString();
        }
    }
}
