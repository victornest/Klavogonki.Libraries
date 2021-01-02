using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface ITopService : IProgressNotifier
    {
        Task<ModeStatList> GetBulkTop(ModeStatSettings input, Period periodType);
        Task<List<TopStat>> GetTop(string modeId, int maxUsers, Period periodType);
    }
}