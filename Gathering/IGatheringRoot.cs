using System.Threading.Tasks;

namespace BudgetReview.Gathering
{
    internal interface IGatheringRoot
    {
        Task<DataSet<RawDataGroup>> Start();

        /// <summary>
        /// Provides a summary of how gathering went. Can be accessed afterwards.
        /// </summary>
        GatheringSummary Summary { get; }
    }
}
