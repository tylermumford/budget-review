using System.Threading.Tasks;

namespace BudgetReview.Gathering
{
    internal interface IGatheringRoot
    {
        Task<DataSet<RawDataItem>> Start();
    }
}
