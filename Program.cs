using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetReview.Analyzing;
using BudgetReview.Gathering;
using BudgetReview.Parsing;

namespace BudgetReview
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConfigureLogging();

            Console.WriteLine("# Budget Review");

            EmitDebugInfo();

            Console.WriteLine("\n## Gathering...");

            var gatherer = new RootGatherer();
            var rawProgramData = await gatherer.Start();

            Debug.WriteLine(rawProgramData.ToString());
            var sourceCount = rawProgramData.Count();
            var lineCount = rawProgramData.Select(d => d.ContentLines.Length).Sum();
            Console.WriteLine($"Gathered {lineCount} lines of data from {sourceCount} sources");

            Debug.WriteLine("\n## Parsing...");

            var parser = new RootParser();
            var parsedData = parser.Parse(rawProgramData);

            lineCount = parsedData.Select(d => d.LineItems.Count()).Sum();
            Console.WriteLine($"Parsed data into {lineCount} transactions");

            Console.WriteLine("\n## Analyzing...");

            var analyzer = new RootAnalyzer(parsedData);
            var analysis = analyzer.Analyze();

            Console.WriteLine("Analyzed all data");
            Console.WriteLine(analysis.GetDisplayString());

            Debug.WriteLine("\n## Creating output file...");

            analysis.WriteToFile();

            Console.WriteLine("Created output file");
        }

        private static void EmitDebugInfo()
        {
            Debug.WriteLine($"Current culture: {CultureInfo.CurrentCulture}");
            Debug.WriteLine($"Current UI culture: {CultureInfo.CurrentUICulture}");
            Debug.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
        }

        private static void ConfigureLogging()
        {
            var l = new ConsoleTraceListener();
            Trace.Listeners.Add(l);
        }
    }
}
