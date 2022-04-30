using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetReview.Analyzing;
using BudgetReview.Gathering;
using BudgetReview.Parsing;
using Serilog;

namespace BudgetReview
{
    /// <summary>This is the "main" logic of the program.</summary>
    internal class BudgetReview
    {
        private IGatheringRoot gatherer;
        private IParser parser;

        public BudgetReview(IGatheringRoot gatheringRoot, IParser parsingRoot)
        {
            gatherer = gatheringRoot;
            parser = parsingRoot;
        }

        public async Task Execute()
        {
            Console.WriteLine("\n## Gathering...");

            var rawProgramData = await gatherer.Start();

            Log.Information("{RawGatheredData}", rawProgramData);
            var sourceCount = rawProgramData.Count();
            var lineCount = rawProgramData.Select(d => d.ContentLines.Length).Sum();
            Console.WriteLine($"Gathered {lineCount} lines of data from {sourceCount} sources");

            Log.Information("Parsing...");

            var parsedData = parser.Parse(rawProgramData);

            lineCount = parsedData.Select(d => d.LineItems.Count()).Sum();
            Console.WriteLine($"Parsed data into {lineCount} transactions");

            Console.WriteLine("\n## Analyzing...");

            var analyzer = new RootAnalyzer(parsedData);
            var analysis = analyzer.Analyze();

            Log.Information("Analyzed all data");
            Console.WriteLine(analysis.GetDisplayString());

            Log.Information("Creating output file...");

            analysis.WriteToFile();

            Log.Information("Created output file");
        }
    }
}
