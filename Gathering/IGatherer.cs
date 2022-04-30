using System.Threading.Tasks;

namespace BudgetReview.Gathering
{
    internal interface IGatherer
    {
        Task GatherInto(DataSet<RawDataGroup> results);
    }
}
