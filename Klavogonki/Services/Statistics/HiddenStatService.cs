using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Klavogonki
{
    public class HiddenStatService : IHiddenStatService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly IQuickStatService quickStatService;

        public HiddenStatService()
        {
            quickStatService = new QuickStatService();
        }

        public event EventHandler<EventArgs<HiddenStat>> PreliminaryHiddenStatUpdated;
        
        private string[] AwardIds { get; set; } = new string[] { };
        private string[] AchiveIds { get; set; } = new string[] { };

        public async Task<HiddenStat> GetHiddenStat(int userId)
        {
            HiddenStat hiddenStat = new HiddenStat(userId);
            ChangeProgress(new Progress(0));
            var qsNormal = await quickStatService.GetQuickStat(userId, "normal");

            this.AwardIds = qsNormal.Value.Awards.Select(x => x.ModeId).ToArray();
            hiddenStat.TotalMileage = qsNormal.Value.TotalMileage;

            string json = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/api/profile/get-achieves?q=progress&skip=0&sort=difficulty&userId=" + userId);
            json = json.Replace("$id", "id");

            var achs = JsonHelper.Deserialize<Achievements>(json);

            List<Tuple<string, double>> userAch = new List<Tuple<string, double>>(32);
            if (achs.Achieves != null)
            {
                foreach (var ach in achs.Achieves)
                {
                    Achievements.ProgressAchievement a = achs.List.Find(x => x.AchieveId.Id == ach.Key);
                    if (a == null) continue;
                    if (ach.Value.Achieve.Name == "Book")
                    {
                        Tuple<string, double> voc = new Tuple<string, double>("voc-" + ach.Value.Achieve.VocId, a.Progress * ach.Value.BookSnippet);// разобраться что это делает
                        userAch.Add(voc);
                    }
                    else if (ach.Value.Achieve.Name == "NumRacesLocal")
                    {
                        Tuple<string, double> voc = new Tuple<string, double>
                            (ach.Value.Achieve.GameType,
                            a.Progress + (a.LevelProgress ?? 0));
                        userAch.Add(voc);
                    }
                }
            }
            this.AchiveIds = userAch.OrderByDescending(x => x.Item2).Select(x => x.Item1).ToArray();
            var modeIds = AwardIds.Union(AchiveIds).ToArray();

            for (int i = 0; i < modeIds.Length; i++)
            {
                ChangeProgress(new Progress(i, modeIds.Length,
                        hiddenStat.MileageCount, hiddenStat.TotalMileage));

                if (!hiddenStat.QuickStats.Exists(x => x.Mode.ModeId == modeIds[i]))
                {
                    var qs = await quickStatService.GetQuickStat(userId, modeIds[i]);
                    hiddenStat.MileageCount += qs.Value.Mileage;
                    hiddenStat.QuickStats.Add(qs.Value);
                }

                if ((i + 1) % 50 == 0)
                {
                    hiddenStat.QuickStats.Sort(new QuickStat.QuickStartMileageComparer());
                    PreliminaryHiddenStatUpdated?.Invoke(this, new EventArgs<HiddenStat>(hiddenStat));
                }
            }
            hiddenStat.QuickStats.Sort(new QuickStat.QuickStartMileageComparer());
            ChangeProgress(new Progress(modeIds.Length, modeIds.Length));

            return hiddenStat;
        }
    }
}
