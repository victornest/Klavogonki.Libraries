using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IPeriodStatService
    {
        Task<FetchResult<List<DayStat>>> GetDaysStat(int userId, string mode, DateTime? from = null, DateTime? to = null, Period periodType = Period.Day);

        PeriodStat GetPeriodStat(List<DayStat> daysStat, DateTime? from = null, DateTime? to = null, int? maxSpeed = null, int targetResultsCount = 5);

        PeriodStat GetPeriodStatLimited(List<DayStat> daysStat, int maxMileage, DateTime? minFrom = null, DateTime? to = null, int? maxSpeed = null, int targetResultsCount = 5);
    }
}