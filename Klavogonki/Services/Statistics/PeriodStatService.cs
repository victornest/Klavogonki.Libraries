using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class PeriodStatService : IPeriodStatService
    {
        public async Task<FetchResult<List<DayStat>>> GetDaysStat(int userId, string mode, DateTime? from = null, DateTime? to = null, Period periodType = Period.Day)
        {
            FetchResult<List<DayStat>> result;

            var _from = from ?? new DateTime(2008, 01, 01);
            var _to = to ?? DateTime.Now;

            string url = $"http://klavogonki.ru/api/profile/get-stats-details-data?userId={userId}&gametype={mode}&fromDate={_from.Date:yyyy-MM-dd}&toDate={_to:yyyy-MM-dd}&grouping={periodType.ToString().ToLower()}";

            try
            {
                string json = await NetworkClient.DownloadstringAsync(url);
                if (json == "{\"err\":\"invalid user id\"}")
                {
                    result = new FetchResult<List<DayStat>>(userExists: false);
                }
                else if (json == "{\"err\":\"permission blocked\"}")
                {
                    result = new FetchResult<List<DayStat>>(isOpen: false);
                }
                else
                {
                    var daysStat = JsonHelper.Deserialize<DaysStatResponse>(json);
                    List<DaysStatResponse.DayResult> daylist = daysStat.List;

                    var list = new List<DayStat>();
                    if (daylist != null)
                    {
                        foreach (var day in daylist)
                        {
                            var dayStat = new DayStat
                            {
                                Date = new DateTime(1970, 1, 1).AddSeconds(day.MinDate.Sec + 14400).Date,
                                Mileage = day.Cnt,

                                AvgErrors = day.AvgErrors,
                                AvgErrorsEnd = day.AvgErrorsEnd,
                                AvgSpeed = day.AvgSpeed,
                                AvgSpeedEnd = day.AvgSpeedEnd,
                                MaxErrors = day.MaxErrors,
                                MaxSpeed = day.MaxSpeed,
                                MinErrors = day.MinErrors,
                                MinSpeed = day.MinSpeed
                            };
                            list.Add(dayStat);
                        }
                    }
                    result = new FetchResult<List<DayStat>>(list);
                }
            }
            catch
            {
                result = new FetchResult<List<DayStat>>(isSuccessfulDownload: false);
            }
            return result;
        }


        public PeriodStat GetPeriodStat(List<DayStat> daysStat, DateTime? from = null, DateTime? to = null, int? maxSpeed = null, int targetResultsCount = 5)
        {
            if (from != null)
                daysStat = daysStat.Where(x => x.Date >= from.Value.Date).ToList();
            if (to != null)
                daysStat = daysStat.Where(x => x.Date <= to.Value.Date).ToList();

            return GetPeriodStat(daysStat, maxSpeed, targetResultsCount);
        }

        public PeriodStat GetPeriodStatLimited(List<DayStat> daysStat, int minMileage, DateTime? minFrom = null, DateTime? to = null, int? maxSpeed = null, int targetResultsCount = 5)
        {
            if (to != null)
                daysStat = daysStat.Where(x => x.Date <= to.Value.Date).ToList();

            var newMaxSpeed = daysStat.Max(x => x.MaxSpeed);
            if (newMaxSpeed < maxSpeed) maxSpeed = newMaxSpeed;

            daysStat = daysStat.OrderByDescending(x => x.Date).ToList();

            List<DayStat> list = new List<DayStat>();
            foreach (var day in daysStat)
            {
                if (list.Sum(x => x.Mileage) >= minMileage
                    && (!minFrom.HasValue || day.Date < minFrom.Value)) 
                    break;

                list.Add(day);
            }
            list.Reverse();

            return GetPeriodStat(list, maxSpeed, targetResultsCount);
        }

        private PeriodStat GetPeriodStat(List<DayStat> daysStat, int? maxSpeed = null, int targetResultsCount = 5)
        {
            var periodStat = new PeriodStat();  

            periodStat.Mileage = daysStat.Sum(x => x.Mileage);

            if (daysStat.Any())
                periodStat.MaxSpeed = daysStat.Max(x => x.MaxSpeed);

            if (daysStat.Count >= targetResultsCount)
            {
                periodStat.RawBestResults = daysStat.Select(x => x.MaxSpeed).OrderByDescending(x => x).Take(targetResultsCount).ToArray();
            }

            var recordStat = daysStat;

            if (maxSpeed != null)
            {
                recordStat = recordStat.Where(x => x.MaxSpeed <= maxSpeed.Value).ToList();
            }
            var maxSpeeds = recordStat.Select(x => x.MaxSpeed).OrderByDescending(x => x).ToList();

            if (maxSpeed != null && maxSpeeds.Any() && maxSpeeds.FirstOrDefault() != maxSpeed.Value)
            {
                maxSpeeds.Insert(0, maxSpeed.Value);
            }

            var array = maxSpeeds.Take(targetResultsCount).ToArray();
            if (array.Length == targetResultsCount)
            {
                periodStat.BestResultsBelowMaxSpeed = array;
            }     
            return periodStat;
        }
    }
}
