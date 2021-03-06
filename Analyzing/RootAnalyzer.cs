using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetReview.Extensions;
using Serilog;

namespace BudgetReview.Analyzing
{
    internal class RootAnalyzer : IAnalyzer
    {
        private readonly AnalysisResult Result = new();
        private DataSet<ParsedDataItem> ParsedData = new();

        public AnalysisResult Analyze(DataSet<ParsedDataItem> parsedData)
        {
            var s = new Stopwatch();
            s.Start();

            ParsedData = parsedData;

            NormalizationAndFilteringStep();
            PaycheckIdentificationStep();
            BasicAnalyzer.Step(Result);

            s.Stop();
            Log.Information("Analysis finished in {Elapsed}", s.Elapsed);
            return Result;
        }

        private void NormalizationAndFilteringStep()
        {
            foreach (var file in ParsedData)
            {
                foreach (var line in file.LineItems)
                {
                    if (ShouldIgnore(line, file))
                        continue;
                    Result.Transactions.Add(new Transaction(line, file.Source));
                }
            }
        }

        private static bool ShouldIgnore(ILineItem line, ParsedDataItem file)
        {
            var ignoreAsMacuTransfer = file.Source == Source.MACU
                && (
                    line.Description.StartsWith("From Share ")
                    || line.Description.StartsWith("To Share ")
                )
                && Env.GetOrThrow("SHOULD_IGNORE_MACU_TRANSFERS") == "1";

            var ignoreAsCardPayment = Env.GetOrThrow("SHOULD_IGNORE_CARD_PAYMENTS") == "1"
                && (
                    line.Description.StartsWith("Payment to Chase Mastercard & Visa")
                    || line.Description.StartsWith("Payment to Citibank")
                    || line.Description.StartsWith("ELECTRONIC PAYMENT-THANK YOU")
                );

            return ignoreAsMacuTransfer || ignoreAsCardPayment;
        }

        private void PaycheckIdentificationStep()
        {
            var macu = Result.Transactions.Where(t => t.Source == Source.MACU);
            var incomeTransactions = macu.Where(t => t.Amount > 0);
            foreach (var t in incomeTransactions)
                t.Category = CategoryName.Of("Income");

            var substrings = SubstringsThatIdentifyIncome();
            foreach (var t in incomeTransactions.Where(t => t.Description.ContainsAny(substrings)))
                t.Description = "Paycheck";
        }

        private IEnumerable<string> SubstringsThatIdentifyIncome()
        {
            var envPreference = Env.Get("PAYCHECK_IDENTIFICATION_SUBSTRINGS");
            var splitter = Env.Get("PAYCHECK_IDENTIFICATION_SPLIT_CHAR", "|");
            var substrings = envPreference.Split(splitter);
            return substrings;
        }
    }
}
