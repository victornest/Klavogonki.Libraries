using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Klavogonki;
using KlStatisticsLoader;
using Newtonsoft.Json;
using Rank = KlStatisticsLoader.Rank;

namespace MigomanUpdater.Services
{
    [Serializable]
    public class UserTopResult
    {
        public int UserId { get; set; }
        public int MaxSpeed { get; set; }
    }
    
    internal class MigomanResultService : IMigomanResultService
    {
        private readonly ITopService topService_;
        private readonly IModeStatService modeStatService_;
        private readonly IQuickStatService quickStatService_;
        
        public MigomanResultService(ITopService topService, IModeStatService modeStatService, IQuickStatService quickStatService)
        {
            topService_ = topService;
            modeStatService_ = modeStatService;
            quickStatService_ = quickStatService;
        }

        public async Task<List<UserMigomanResult>> GetResultsAsync(List<string> modeIds, List<string> allTimeMileageModeIds, DateTime fromDate, DateTime toDate)
        {
            var userResultsById = new Dictionary<int, UserMigomanResult>();

            var i = 0;

            var serializer = new JsonSerializer();

            foreach (var modeId in modeIds)
            {
                var dayModeStatSettings = new ModeStatSettings()
                {
                    ModeIds = new List<string>
                    {
                        modeId
                    },
                    MaxUsers = 100000,
                    NeedPeriodStat = false,
                    NeedQuickStat = false
                };
                
                var path = $"{modeId}-stats.json";
                var fileWithDayTopsExists = File.Exists(path);
                string fileDayTopResultsJson;
                List<UserTopResult> fileDayTopResults;
                var dayTopStatList = await topService_.GetBulkTop(dayModeStatSettings, Period.Day);
                var dayTopResults = dayTopStatList[0].Select(r => new UserTopResult
                    {UserId = r.UserId, MaxSpeed = r.TopResult ?? 0}).ToList();
                
                var dayTopResultsJson = JsonHelper.Serialize(dayTopResults);
                if (fileWithDayTopsExists)
                {
                    fileDayTopResultsJson = File.ReadAllText(path);
                    fileDayTopResults = JsonHelper.Deserialize<List<UserTopResult>>(fileDayTopResultsJson);
                }
                else
                {
                    fileDayTopResults = dayTopResults;
                }
                

                if (fileWithDayTopsExists)
                {
                    var resultsToAdd = new List<UserTopResult>();
                    foreach (var dayTopResult in dayTopResults)
                    {
                        var userId = dayTopResult.UserId;
                        var fileDayTopResult = fileDayTopResults.SingleOrDefault(r => r.UserId == userId);
                        if (fileDayTopResult != null)
                        {
                            if (dayTopResult.MaxSpeed > fileDayTopResult.MaxSpeed)
                            {
                                fileDayTopResult.MaxSpeed = dayTopResult.MaxSpeed;
                            }
                        }
                        else
                        {
                            resultsToAdd.Add(dayTopResult);
                        }
                    }
                    fileDayTopResults.AddRange(resultsToAdd);
                }

                fileDayTopResultsJson = JsonHelper.Serialize(fileDayTopResults);
                File.WriteAllText(path, fileDayTopResultsJson);
                
                
                if (i > 0 && userResultsById.Count == 0)
                {
                    break;
                }
                var modeStatSettings = new ModeStatSettings()
                {
                    DateFrom = fromDate,
                    DateTo = toDate,
                    ModeIds = new List<string>
                    {
                        modeId
                    },
                    UserIds = i == 0 ? null : userResultsById.Keys.ToList(),
                    MaxUsers = 100000,
                    NeedPeriodStat = true,
                    NeedQuickStat = true
                };

                var topResults = i == 0 
                    ? await topService_.GetBulkTop(modeStatSettings, Period.Week)
                    : await modeStatService_.GetStatsByUsersAndModes(modeStatSettings);
                var topResult = topResults[0];

                var gameType = topResult.Mode.ModeId;
                foreach (var userStat in topResult)
                {
                    // todo check if it is because of 502 bad gateway
                    if (userStat.PeriodStat == null)
                    {
                        continue;
                    }
                    if (!userStat.PeriodStat.MaxSpeed.HasValue)
                    {
                        continue;
                    }
                    
                    var requiredTornamentMileage = 1;
                    
                    if (userStat.PeriodStat.Mileage < requiredTornamentMileage)
                    {
                        continue;
                    }

                    if (userStat.PeriodStat.MaxSpeed > userStat.QuickStat.Record)
                    {
                        if (userStat.PeriodStat.BestResultsBelowMaxSpeed.Length == 0)
                        {
                            continue;
                        }

                        userStat.PeriodStat.MaxSpeed = userStat.PeriodStat.BestResultsBelowMaxSpeed[0];
                    }
                    
                    if (!userResultsById.TryGetValue(userStat.UserId, out var userMigomanResult))
                    {
                        if (i > 0)
                        {
                            continue; // should be unreachable
                        }
                        userMigomanResult = new UserMigomanResult();
                        userResultsById[userStat.UserId] = userMigomanResult;
                    }
                    userMigomanResult.UserId = userStat.UserId;
                    userMigomanResult.UserName = userStat.QuickStat.Nick;

                    var bestSpeedDayTop = fileDayTopResults.SingleOrDefault(r => r.UserId == userStat.UserId);
                    
                    userMigomanResult.BestTournamentSpeeds[gameType] = bestSpeedDayTop?.MaxSpeed ?? userStat.PeriodStat.MaxSpeed.Value;

                    //TODO improve
                    if (modeId == GameType.NormalEn)
                    {
                        userMigomanResult.BestAllTimeMainSpeed = userStat.QuickStat.Record; 
                    }

                    userMigomanResult.TournamentMileages[gameType] = userStat.PeriodStat.Mileage;
                    userMigomanResult.AllTimeMileages[gameType] = userStat.QuickStat.Mileage;
                }
                
                i++;
                var keysToRemove = userResultsById.Where(r => r.Value.BestTournamentSpeeds.Count < i).Select(p => p.Key).ToList();
                keysToRemove.ForEach(k => userResultsById.Remove(k));
            }

            foreach (var userMigomanResult in userResultsById)
            {
                foreach (var allTimeMileageModeId in allTimeMileageModeIds)
                {
                    var quickStatNormalRu = await quickStatService_.GetQuickStat(userMigomanResult.Key, allTimeMileageModeId);
                    if (!quickStatNormalRu.IsSuccessfulDownload || quickStatNormalRu.Value == null)
                    {
                        continue;
                    }
                    userMigomanResult.Value.AllTimeMileages[allTimeMileageModeId] = quickStatNormalRu.Value.Mileage;                    
                }
                
            }
            
            return userResultsById.Values.Where(v => v.Rank >= Rank.TaxiDriver).OrderByDescending(v => v.BestTournamentSpeedsTotal).ToList();
        }
    }
}