using System;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IDetailedStatService : IProgressNotifier
    {
        Task<FetchResult<RaceResults>> GetDetailedStat(int userId, string modeId);

        Task<FetchResult<RaceResults>> GetDayStat(int userId, string modeId, DateTime date);
    }
}