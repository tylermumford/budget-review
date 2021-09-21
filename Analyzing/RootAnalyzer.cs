using System;
using System.Diagnostics;
using System.Linq;

namespace BudgetReview.Analyzing
{
    internal class RootAnalyzer
    {
        public DataSet<ParsedDataItem> ParsedData { get; init; }
        private readonly AnalysisResult Result = new();

        public RootAnalyzer(DataSet<ParsedDataItem> parsedData)
        {
            ParsedData = parsedData;
        }

        public AnalysisResult Analyze()
        {
            var s = new Stopwatch();
            s.Start();

            NormalizationAndFilteringStep();
            IncomeStep();
            BasicAnalyzer.Step(Result);

            s.Stop();
            Debug.WriteLine($"Analysis finished in {s.Elapsed}");
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
                && Constants.ShouldIgnoreMacuTransfers;

            var ignoreAsCardPayment = Constants.ShouldIgnoreCardPayments
                && (
                    line.Description.StartsWith("Payment to Chase Mastercard & Visa")
                    || line.Description.StartsWith("Payment to Citibank")
                    || line.Description.StartsWith("ELECTRONIC PAYMENT-THANK YOU")
                );

            return ignoreAsMacuTransfer || ignoreAsCardPayment;
        }

        private void IncomeStep()
        {
            var macu = Result.Transactions.Where(t => t.Source == Source.MACU);
            var incomeTransactions = macu.Where(t => t.Amount > 0);
            foreach (var t in incomeTransactions)
                t.Category = Category.ByName("Income");

            foreach (var t in incomeTransactions.Where(t => t.Description == "Mana Deposit"))
                t.Description = "ManagerPlus paycheck";
        }
    }
}