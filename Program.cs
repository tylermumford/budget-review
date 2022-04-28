using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetReview.Analyzing;
using BudgetReview.Gathering;
using BudgetReview.Parsing;
using Serilog;
using Serilog.Events;

namespace BudgetReview
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Env.Load();
            ConfigureLogging();

            Console.WriteLine("# Budget Review");

            EmitDebugInfo();

            try
            {
                var clock = new Stopwatch();
                clock.Start();

                await PerformBudgetReview();

                Log.Debug("Budget review took {Duration} to run", clock.Elapsed);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task PerformBudgetReview()
        {
            Console.WriteLine("\n## Gathering...");

            var gatherer = new GatheringRoot();
            var rawProgramData = await gatherer.Start();

            Log.Information("{RawGatheredData}", rawProgramData);
            var sourceCount = rawProgramData.Count();
            var lineCount = rawProgramData.Select(d => d.ContentLines.Length).Sum();
            Console.WriteLine($"Gathered {lineCount} lines of data from {sourceCount} sources");

            Log.Information("Parsing...");

            var parser = new RootParser();
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

        private static void EmitDebugInfo()
        {
            Log.Debug($"Current culture: {CultureInfo.CurrentCulture}");
            Log.Debug($"Current UI culture: {CultureInfo.CurrentUICulture}");
            Log.Debug("Current directory: {CurrentDirectory}", Directory.GetCurrentDirectory());
            Log.Debug("Env file parsed: {IsEnvParsed}", Env.Get("env_file_parsed", "No"));
            Log.Debug("Command line args: {Args}", Environment.GetCommandLineArgs());
        }

        private static void ConfigureLogging()
        {
            var configuredMin = Env.Get("min_log_level", "Warning");
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(Enum.Parse<LogEventLevel>(configuredMin))
                .WriteTo.Console(outputTemplate: "[{Level:u4}] {Message:lj}{NewLine}{Exception}");

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}
