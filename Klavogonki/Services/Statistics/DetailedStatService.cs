using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class DetailedStatService : IDetailedStatService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress) => ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));

        public async Task<FetchResult<RaceResults>> GetDetailedStat(int userId, string modeId)
        {
            ChangeProgress(new Progress(0));

            string address = $"http://klavogonki.ru/api/profile/get-stats-details-data?userId={userId}&gametype={modeId}&fromDate=2008-01-01&toDate=2030-01-01&grouping=day";
            string json = await NetworkClient.DownloadstringAsync(address);

            if (json.Contains("permission blocked"))
            {
                return new FetchResult<RaceResults>();
            }


            DaysStatResponse pageDays = JsonHelper.Deserialize<DaysStatResponse>(json);
            List<DaysStatResponse.DayResult> daylist = pageDays.List;

            var allresultlist = new RaceResults();
            for (int i = 0; i < daylist.Count; i++)
            {
                ChangeProgress(new Progress(i, daylist.Count));
                string datestr = daylist[i].MinDate.Date.ToString("yyyy'-'MM'-'dd");
                var address2 = $"http://klavogonki.ru/api/profile/get-stats-details-data?userId={userId}&gametype={modeId}&fromDate={datestr}&toDate={datestr}&grouping=none";
                json = await NetworkClient.DownloadstringAsync(address2);
                if (json == "{\"err\":\"not pro\"}")
                {
                    return new FetchResult<RaceResults>();
                }
                
                DayRacesResponse pageDay = JsonHelper.Deserialize<DayRacesResponse>(json);
                allresultlist.AddRange(pageDay.List);
            }
            ChangeProgress(new Progress(daylist.Count, daylist.Count));
            return new FetchResult<RaceResults>(allresultlist);
        }

        public async Task<FetchResult<RaceResults>> GetDayStat(int userId, string modeId, DateTime date)
        {
            FetchResult<RaceResults> result;

            string datestr = date.Date.ToString("yyyy'-'MM'-'dd");
            var address2 = $"http://klavogonki.ru/api/profile/get-stats-details-data?userId={userId}&gametype={modeId}&fromDate={datestr}&toDate={datestr}&grouping=none";
            try
            {
                string json = await NetworkClient.DownloadstringAsync(address2);

                if (json == "{\"err\":\"invalid user id\"}")
                {
                    result = new FetchResult<RaceResults>(userExists: false);
                }
                else if (json == "{\"err\":\"permission blocked\"}")
                {
                    // we don't know exactly if he has premium
                    result = new FetchResult<RaceResults>(isOpen: false);
                }
                else if (json == "{\"err\":\"not pro\"}")
                {
                    result = new FetchResult<RaceResults>(isOpen: true);
                } else if (json == "{\"err\":\"invalid gametype\"}")
                {
                    result = new FetchResult<RaceResults>(isOpen: true);
                }
                else
                {
                    DayRacesResponse pageDay = JsonHelper.Deserialize<DayRacesResponse>(json);
                    var list = new RaceResults() { HasPremium = true };
                    list.AddRange(pageDay.List);
                    result = new FetchResult<RaceResults>(list);
                }
            }
            catch
            {
                result = new FetchResult<RaceResults>(isSuccessfulDownload: false);
            }
            return result;
        }
    }
}
