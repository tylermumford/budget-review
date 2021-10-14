using System.Threading.Tasks;

namespace BudgetReview.Gathering
{
    internal interface IGatherer
    {
        Task GatherInto(DataSet<RawDataItem> results);
    }
}
