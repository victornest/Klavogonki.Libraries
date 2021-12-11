using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KlStatisticsLoader;

namespace MigomanUpdater.Services
{
    internal interface IGoogleSheetService
    {
        Task UploadUsersResultsToSheetsAsync(List<string> modeIds, Dictionary<string, double> coeffs, List<UserMigomanResult> usersResults, DateTime startTimestamp);
    }
}