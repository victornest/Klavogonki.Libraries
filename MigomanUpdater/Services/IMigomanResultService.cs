using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KlStatisticsLoader;

namespace MigomanUpdater.Services
{
    internal interface IMigomanResultService
    {
        Task<List<UserMigomanResult>> GetResultsAsync(List<string> modeIds, List<string> allTimeMileageModeIds, DateTime fromDate, DateTime toDate);
    }
}