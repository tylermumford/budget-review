using System;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;

namespace BudgetReview.Analyzing
{
    internal static class AnalysisOutputExtension
    {
        public static void WriteToFile(this AnalysisResult results)
        {
            using var file = ChooseFile();
            var config = ChooseConfig();
            using var writer = new CsvWriter(file, config);
            writer.Context.RegisterClassMap<TransactionMap>();

            var items = results.Transactions
                .OrderBy(t => t.Date);
            writer.WriteRecords(items);
        }

        private static StreamWriter ChooseFile()
        {
            var guid = Guid.NewGuid();
            var name = $"Budget-Review-Analysis-{guid}.csv";
            var file = File.CreateText(name);
            Console.WriteLine($"Creating output file {name}");
            return file;
        }

        private static CsvConfiguration ChooseConfig()
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture);
            config.NewLine = "\n";
            return config;
        }
    }
}
