using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static BudgetReview.Gathering.MacuGatherer;
using static BudgetReview.Gathering.AmazonGatherer;
using static BudgetReview.Gathering.CitiCardGatherer;
using static BudgetReview.Gathering.ChaseCardGatherer;

namespace BudgetReview.Gathering
{
    internal class RootGatherer
    {

        public Task<DataSet<RawDataItem>> Start()
        {
            return FromLocalFolder(Constants.RawDataDir);
        }

        private static Task<DataSet<RawDataItem>> FromLocalFolder(string dir)
        {
            if (!Directory.Exists(dir))
            {
                throw new ArgumentException("directory doesn't exist", nameof(dir));
            }

            Debug.WriteLine($"{Directory.GetFiles(dir).Length} files in {dir}");

            var result = new DataSet<RawDataItem>();
            AddMacuTransactions(result);
            AddAmazonTransactions(result, dir);
            AddCitiCardTransactions(result);
            AddChaseCardTransactions(result);
            return Task.FromResult(result);
        }
    }
}
