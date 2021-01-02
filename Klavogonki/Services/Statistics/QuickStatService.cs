using System.Text.RegularExpressions;
using System;
using HtmlAgilityPack;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class QuickStatService : IQuickStatService
    {
        public async Task<FetchResult<QuickStat>> GetQuickStat(
            int id,
            string modeId = "normal",
            bool needAwards = true)
        {
            FetchResult<QuickStat> result;
            try
            {
                QuickStat qs = new QuickStat();
                qs.Id = id;
                modeId = new Mode(modeId).ModeId;
                string html = await NetworkClient.DownloadstringAsync($"http://klavogonki.ru/ajax/profile-popup?user_id={id}&gametype={modeId}");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                string sLevel = doc.DocumentNode.SelectSingleNode(".//div[@class='level_icon']").InnerText;
                qs.Level = int.Parse(sLevel);

                var mRank = doc.DocumentNode.SelectSingleNode(".//table/tr/td/div");
                qs.Rank = Rank.GetByIndex(int.Parse(mRank.Attributes["class"].Value.Replace("rang", "")));
                qs.Nick = doc.DocumentNode.SelectSingleNode(".//div[@class='name']").InnerText;
                string sOverall = doc.DocumentNode.SelectSingleNode(".//table[2]/tr/td").InnerText;
                qs.TotalMileage = int.Parse(Regex.Match(sOverall, "\\d+").Value);

                string _modename = doc.DocumentNode.SelectSingleNode(".//table[2]/tr[3]").InnerText;
                string modeName = Regex.Match(_modename, "&laquo;(.+)&raquo").Groups[1].Value;
                qs.Mode = new Mode(modeId, modeName);

                string posTxts = doc.DocumentNode.SelectSingleNode(".//table[2]/tr[4]/td").InnerText.Trim();
                string[] _poss = posTxts.Split('|');
                int temp;
                if (_poss.Length == 2)
                {
                    qs.DayTop = int.TryParse(_poss[0], out temp) ? temp : (int?)null; //топ дня
                    qs.WeekTop = int.TryParse(_poss[1], out temp) ? temp : (int?)null; //топ недели
                }
                else if (_poss.Length == 1)
                    qs.BookTop = int.TryParse(_poss[0], out temp) ? temp : (int?)null; //топ книги

                string sRecord = doc.DocumentNode.SelectSingleNode(".//table[2]/tr[5]/td").InnerText;
                qs.Record = int.Parse(Regex.Match(sRecord, "\\d+").Value);

                string sAverage = doc.DocumentNode.SelectSingleNode(".//table[2]/tr[6]/td").InnerText;
                qs.AvgSpeed = int.Parse(Regex.Match(sAverage, "\\d+").Value);

                string sErrors = doc.DocumentNode.SelectSingleNode(".//table[2]/tr[7]/td").InnerText;

                qs.AvgErRate = double.Parse(sErrors.Replace("%", ""), new NumberFormatInfo() { NumberDecimalSeparator = "," }) / 100;

                string sMileage = doc.DocumentNode.SelectSingleNode(".//table[2]/tr[8]/td").InnerText;
                MatchCollection matches = Regex.Matches(sMileage, "\\d+");
                qs.Mileage = int.Parse(matches[0].Value);
                qs.Time = new TimeSpan();
                if (matches.Count == 3)
                {
                    qs.Time = qs.Time.Add(TimeSpan.FromHours(int.Parse(matches[1].Value)));
                    qs.Time = qs.Time.Add(TimeSpan.FromMinutes(int.Parse(matches[2].Value)));
                }
                else if (matches.Count == 2)
                    qs.Time = qs.Time.Add(TimeSpan.FromMinutes(int.Parse(matches[1].Value)));


                if (needAwards)
                {
                    var nodes = doc.DocumentNode.SelectNodes(".//table[2]/tr/td/a");
                    if (nodes != null && nodes.Count > 0)
                    {
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            string sModeId = nodes[i].Attributes["href"].Value;
                            var aModeId = Regex.Match(sModeId, "(?<=gametype=).+$").Value;

                            string style = nodes[i].GetAttributeValue("style", "");
                            var match = Regex.Match(style, @"(-\d+)px 0;$");
                            int margin = 0;
                            if (match.Success)
                            {
                                margin = int.Parse(match.Groups[1].Value);
                            }
                            AwardType type = GetAwardTypeByMargin(margin);
                            Award award = new Award() { ModeId = aModeId, Type = type };
                            qs.Awards.Add(award);
                        }
                        qs.BooksGold = qs.Awards.Count(x => x.Type ==  AwardType.GoldenBook);
                        qs.BooksSilver = qs.Awards.Count(x => x.Type == AwardType.SilverBook);
                        qs.BooksBronze = qs.Awards.Count(x => x.Type == AwardType.BronzeBook);
                    }
                }
                result = new FetchResult<QuickStat>(qs);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                result = new FetchResult<QuickStat>(isSuccessfulDownload: false);
            }
            return result;
        }

        private AwardType GetAwardTypeByMargin(int margin)
        {
            AwardType result = AwardType.Medal; ;
            switch (margin)
            {
                case 0:
                    result = AwardType.Medal;
                    break;
                case -10:
                    result = AwardType.Star;
                    break;
                case -20:
                    result = AwardType.Cup;
                    break;
                case -30:
                    result = AwardType.Crown;
                    break;
                case -40:
                    result = AwardType.GoldenHelmet;
                    break;
                case -50:
                    result = AwardType.DiamondHelmet;
                    break;
                case -60:
                    result = AwardType.GrapheneHelmet;
                    break;
                case -70:
                    result = AwardType.Wheel;
                    break;
                case -80:
                    result = AwardType.BronzeBook;
                    break;
                case -90:
                    result = AwardType.SilverBook;
                    break;
                case -100:
                    result = AwardType.GoldenBook;
                    break;
            }
            return result;
        }
    }
}
