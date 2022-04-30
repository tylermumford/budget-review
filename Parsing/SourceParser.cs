using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Serilog;

namespace BudgetReview.Parsing
{
    internal static class SourceParser<T> where T : ILineItem
    {
        public static ParsedDataItem Parse(RawDataGroup item, Source source)
        {
            var result = new ParsedDataItem
            {
                Source = source,
                LineItems = ExtractLineItems(item.ContentLines),
            };
            Log.Information("Parsed {LineCount} {Source} lines, including {FirstItemParsed}", result.LineItems.Count(), source, result.LineItems.FirstOrDefault());
            return result;
        }

        private static IEnumerable<ILineItem> ExtractLineItems(string[] fileLines)
        {
            var r = new StringReader(string.Join('\n', fileLines));
            using var csv = new CsvReader(r, CultureInfo.CurrentUICulture);
            return (IEnumerable<ILineItem>)csv.GetRecords<T>().ToList();
        }
    }
}
