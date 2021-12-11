using HtmlAgilityPack;
using Klavogonki;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class TopService : ITopService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly IQuickStatService quickStatService;
        private readonly IPeriodStatService periodStatService;

        public TopService(IQuickStatService quickStatService, IPeriodStatService periodStatService)
        {
            this.quickStatService = quickStatService;
            this.periodStatService = periodStatService;
        }

        // Получение топа дня/недели
        public async Task<ModeStatList> GetBulkTop(ModeStatSettings input, Period period)
        {
            ChangeProgress(new Progress(0));

            var statType = new StatType()
            {
                HasQuickStat = input.NeedQuickStat,
                HasPeriod = input.NeedPeriodStat,
                HasDayTopResult = period == Period.Day,
                HasWeekTopResult = period == Period.Week
            };
            ModeStatList results = new ModeStatList(statType);            


            foreach (var modeId in input.ModeIds)
            {
                int top_maxpages = (int)Math.Round(input.MaxUsers / 30.0, 0);
                int progressTotal = top_maxpages * 30;

                List<TopStat> topResults = await GetTop(modeId, input.MaxUsers, period);

                ModeStat modeStat = new ModeStat(new Mode(modeId));
                foreach(var player in topResults)
                {
                    modeStat.Add(new Stat(player.Id) { TopResult = player.Result });
                }
                results.Add(modeStat);
                ChangeProgress(new Progress(input.ModeIds.Count, input.ModeIds.Count, progressTotal, progressTotal));
            }

            foreach(var modeStat in results)
            {
                foreach(var stat in modeStat)
                {
                    if (input.NeedQuickStat)
                    {
                        stat.QuickStat = (await quickStatService.GetQuickStat(stat.UserId, modeStat.Mode.ModeId)).Value;
                    }

                    if (input.NeedPeriodStat && input.DateFrom != null && input.DateTo != null)
                    {
                        //http://klavogonki.ru/api/profile/get-stats-details-data?userId=231371&gametype=normal&fromDate=2016-01-08&toDate=2016-02-07&grouping=day

                        var daysStat = await periodStatService.GetDaysStat(stat.UserId, modeStat.Mode.ModeId, input.DateFrom.Value, to: input.DateTo.Value);
                        if (daysStat.Value == null)
                        {
                            continue;
                        }
                        var periodStat = periodStatService.
                            GetPeriodStat(daysStat.Value, from: input.DateFrom.Value, to: input.DateTo.Value, input.NeedQuickStat ? stat.QuickStat.Record : (int?)null);

                        stat.PeriodStat = periodStat;
                    }
                }
            }
            return results;
        }

        public async Task<List<TopStat>> GetTop(string modeId, int maxUsers, Period period)
        {
            var result = new List<TopStat>();

            int maxPage = (int)Math.Ceiling(maxUsers / 30.0);

            Mode mode = new Mode(modeId);
            bool last = false;
            for (int j = 1; j <= maxPage && !last; j++)
            {
                string html;
                if (!mode.IsVoc)
                    html = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/top/" + period.ToString().ToLower() + "/" + mode.ModeId + "/" + j.ToString());
                else
                {
                    html = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/vocs/" + mode.VocId + "/top/" + period.ToString().ToLower() + "/" + j.ToString());
                }
                last = !html.Contains("\">следующая</a>");

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var _table = doc.DocumentNode.SelectSingleNode("//div[@id='toplist'] | //dl[@id='toplist']");
                HtmlNodeCollection _players = _table.SelectNodes(".//tr[@class='other']");
                foreach (var plNode in _players) //30 штук
                {
                    //ChangeProgress(new Progress(modeCounter, input.ModeIds.Count, list.Count, progressTotal));

                    if (string.IsNullOrEmpty(mode.Name))
                    {
                        HtmlNode modeName = null;
                        if (!mode.IsVoc) modeName = doc.DocumentNode.SelectSingleNode("//*[@id='toplist']//li[@class='active']");
                        else modeName = doc.DocumentNode.SelectSingleNode("//*[@id='content']//td[@class='title']/text()");

                        if (modeName != null) mode.SetName(modeName.InnerText.Trim());
                    }

                    var posNode = plNode.SelectSingleNode("td[@class='pos']");
                    var nameNode = plNode.SelectSingleNode("td[@class='name']/a");

                    var resultNode = plNode.SelectSingleNode("td[@class='highlight']/strong"); // словарь
                    var progrNode = plNode.SelectSingleNode("td[@class='highlight progress']/div"); // книга

                    var averageNode = plNode.SelectSingleNode("td/strong");
                    var errorsNode = plNode.SelectSingleNode("td[6]");

                    int position = int.Parse(posNode.InnerText.Replace(".", ""));
                    int id = int.Parse(Regex.Match(nameNode.Attributes["onmouseover"].Value, "\\d+").Value);
                    string nick = nameNode.InnerText.Trim();

                    int resultOrProgress = 0;
                    int snippetsCnt = 1;
                    if (resultNode != null) resultOrProgress = int.Parse(resultNode.InnerText);
                    if (progrNode != null)
                    {
                        MatchCollection matches = Regex.Matches(progrNode.Attributes["title"].Value, "\\d+");
                        resultOrProgress = int.Parse(matches[0].Value);
                        snippetsCnt = int.Parse(matches[1].Value);
                    }

                    int average = int.Parse(averageNode.InnerText);
                    string errorsStr = errorsNode.InnerText;
                    double errors = double.Parse(errorsStr.Replace("%", ""), new NumberFormatInfo() { NumberDecimalSeparator = "," }) / 100;


                    var topResult = new TopStat { Id = id, Nick = nick, Position = position, Result = resultOrProgress, AvgSpeed = average, AvgErRate = errors, TotalSnippets = snippetsCnt };
                    result.Add(topResult);
                    //Stat stat = new Stat(id) { TopResult = resultOrProgress };

                    //if (input.NeedQuickStat)
                    //{
                    //    stat.QuickStat = await quickStatService.GetQuickStat(id, mode.ModeId).Value;
                    //}

                    //PeriodStat periodStat = null;
                    //if (input.NeedPeriodStat && input.DateFrom != null && input.DateTo != null)
                    //{//http://klavogonki.ru/api/profile/get-stats-details-data?userId=231371&gametype=normal&fromDate=2016-01-08&toDate=2016-02-07&grouping=day
                    //    stat.PeriodStat = periodStatService.GetPeriodStat(id, mode.ModeId, input.DateFrom.Value, input.DateTo.Value).Value;
                    //}

                    //list.Add(stat);
                }

            }
            return result;
        }
    }
}
