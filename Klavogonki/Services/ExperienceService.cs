using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class ExperienceService : IExperienceService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private static readonly IStorageProvider<Dictionary<string, List<Experience>>> dataProvider = new StorageProvider<Dictionary<string, List<Experience>>>("db_experience.dat");

        public static Dictionary<string, List<Experience>> ExperienceDB { get; private set; }

        static ExperienceService()
        {
            ExperienceDB = dataProvider.Read();
        }

        public async Task UpdateExperienceData(int maxPages)
        {
            ChangeProgress(new Progress(0, 0, 0));

            DateTime startDate = new DateTime(2009, 11, 1);
            var totalMonths = (DateTime.Now.Month - startDate.Month) + 12 * (DateTime.Now.Year - startDate.Year);

            int monthCounter = 0;

            for (DateTime dt = DateTime.Now.AddMonths(-1);
                dt >= startDate;
                dt = dt.AddMonths(-1))
            {
                int progressTotalPages = 0;
                for (int p = 1; p <= maxPages; p++)
                {
                    ChangeProgress(new Progress(monthCounter, totalMonths, p - 1, progressTotalPages));


                    string yyyyMM = dt.ToString("yyyyMM");
                    if (!ExperienceDB.ContainsKey(yyyyMM)) ExperienceDB.Add(yyyyMM, new List<Experience>(300));

                    int usersInDb = ExperienceDB[yyyyMM].Count == 0 ? 0 : ExperienceDB[yyyyMM].Max(x => x.Position);
                    int usersCountOnPages = p * 30;
                    if (usersInDb >= usersCountOnPages) continue;

                    HtmlDocument doc = new HtmlDocument();
                    string html = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/top/rating/archive-" + yyyyMM + "/" + p);
                    doc.LoadHtml(html);
                    HtmlNodeCollection players = doc.DocumentNode.SelectNodes("//div[@class='content-col']//tr[@class='other']");
                    if (players == null) break;

                    if (progressTotalPages == 0)
                    {
                        var lastPageNode = doc.DocumentNode.SelectSingleNode("//div[@class='pages']/span[last()]");
                        if (lastPageNode != null)
                            progressTotalPages = int.Parse(lastPageNode.InnerText);
                    }

                    foreach (var pl in players)
                    {
                        int position = int.Parse(pl.SelectSingleNode("td[@class='pos']").InnerText.Replace(".", ""));
                        string nick = pl.SelectSingleNode("td[@class='name']").InnerText.Trim();
                        int id = int.Parse(Regex.Match(pl.SelectSingleNode("td[@class='name']/a").Attributes["onmouseover"].Value, "\\d+").Value.ToString());
                        int experience = int.Parse(pl.SelectSingleNode("td[@class='highlight']").InnerText);
                        int competitions = int.Parse(pl.SelectSingleNode("td[6]").InnerText);
                        int averExperience = competitions == 0 ? 0 : experience / competitions;
                        int.TryParse(pl.SelectSingleNode("td[@class='bonuses']").InnerText, out int bonuses);

                        var entry = ExperienceDB[yyyyMM].FirstOrDefault(x => x.Position == position);
                        if (entry == null)
                        {
                            Experience exp = new Experience()
                            {
                                Position = position,
                                Nick = nick,
                                Id = id,
                                ExperienceSum = experience,
                                Competitions = competitions,
                                Bonuses = bonuses,
                                AvgExperience = averExperience
                            };
                            ExperienceDB[yyyyMM].Add(exp);
                        }
                    }
                    if (!html.Contains("\">следующая</a>")) break;
                }
                monthCounter++;
                dataProvider.Save(ExperienceDB);
            }

            dataProvider.Save(ExperienceDB);
            ChangeProgress(new Progress(totalMonths, totalMonths));
        }


        public static List<Experience> GetExperienceHistory(int userId)
        {
            List<Experience> list = new List<Experience>();
            foreach (var month in ExperienceDB)
            {
                Experience found = month.Value.FirstOrDefault(x => x.Id == userId);
                if (found == null) continue;
                found.Date = month.Key;
                list.Add(found);
            }
            var comp = new Comparison<Experience>((x, y) => -x.Date.CompareTo(y.Date));
            list.Sort(comp);
            return list;
        }

        public static List<Experience> GetExperienceTop()
        {
            List<Experience> results = new List<Experience>();

            foreach (var month in ExperienceDB)
            {
                foreach (var experience in month.Value)
                {
                    var player = results.FirstOrDefault(x => x.Id == experience.Id);

                    if (player == null)
                    {
                        results.Add(new Experience()
                        {
                            Id = experience.Id,
                            Nick = experience.Nick,
                            ExperienceSum = experience.ExperienceSum,
                            Competitions = experience.Competitions,
                            Bonuses = experience.Bonuses
                        });
                    }
                    else
                    {
                        player.ExperienceSum += experience.ExperienceSum;
                        player.Competitions += experience.Competitions;
                        player.Bonuses += experience.Bonuses;
                    }
                }
            }

            results.Sort((x, y) => y.ExperienceSum.CompareTo(x.ExperienceSum));
            int counter = 1;
            foreach (var result in results)
            {
                result.AvgExperience = result.ExperienceSum / result.Competitions;
                result.Position = counter++;
            }
            return results;
        }
    }
}
