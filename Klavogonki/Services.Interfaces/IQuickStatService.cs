using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IQuickStatService
    {
        Task<FetchResult<QuickStat>> GetQuickStat(int id, string modeId = "normal", bool needAwards = true);
    }
}