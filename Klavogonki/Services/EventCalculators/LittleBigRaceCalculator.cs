using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class LittleBigRaceCalculator : IProgressNotifier
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;
        protected void ChangeProgress(int progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(new Progress(progress)));
        }

        private readonly IQuickStatService quickStatService = new QuickStatService();
        private readonly IPeriodStatService periodStatService = new PeriodStatService();

        public async Task Calculate(IEnumerable<LBRPlayer> inputPlayers, int minRaces)
        {
            var lbrSettings = GetSettings();
            foreach (var p in inputPlayers)
            {
                p.TypingPoints = (int)p.Results.Sum(x => x.Value.Speed * x.Value.Time.TotalMinutes * 0.04 * (1 + x.Value.PointsIncrease / 100));
            }

            var successfulPlayers = inputPlayers.Where(x => x.IsSuccessful).ToList();
            foreach (var p in successfulPlayers)
            {
                var marStat = (await quickStatService.GetQuickStat(p.Id, Mode.Marathon, false)).Value;
                p.Stats[Mode.Marathon] = marStat;

                p.HasEnoughRacesBySpeed = p.Results.Count(x => x.Value.Speed >= p.Stats[Mode.Marathon].Record * 0.5) >= minRaces;

                if (lbrSettings.StartDate != null && lbrSettings.EndDate != null)
                {
                    var daysStat = await periodStatService.GetDaysStat(p.Id, Mode.Marathon, lbrSettings.StartDate, lbrSettings.EndDate);

                    if (daysStat.IsOpen)
                        p.MarathonPeriodMileage = daysStat.Value.Sum(x => x.Mileage);
                }
            }

            var playersWithEnoughRacesBySpeed = successfulPlayers.Where(x => x.HasEnoughRacesBySpeed).ToList();
            //var brPlayers = GetBigRacePlayers();

            var medIndex = playersWithEnoughRacesBySpeed.Count / 2 - 1;
            if (medIndex < 0) medIndex = 0;

            int counter = 0;
            foreach (var p in playersWithEnoughRacesBySpeed)
            {
                p.Ranks = new LBRPlayer.LBRRanks();
                int counter2 = 1;
                p.BestResults = p.Results.OrderByDescending(x => x.Value.Speed * 1000 + x.Value.ErRate).Take(minRaces).OrderBy(x => x.Key).ToDictionary(x => counter2++, x => x.Value);

                p.BestResultsAvgSpeed = p.BestResults.Average(x => x.Value.Speed);
                p.BestResultsAvgErRate = p.BestResults.Average(x => x.Value.ErRate);

                p.Ranks.Speed = 2;
                p.Ranks.ErRate = 2;

                p.Ranks.MarathonMileage = p.Stats[Mode.Marathon].Mileage >= 500 ? 1 : p.Stats[Mode.Marathon].Mileage >= 100 ? 2 : 3;

                var norStat = (await quickStatService.GetQuickStat(p.Id, Mode.Normal, false)).Value;
                p.Stats[Mode.Normal] = norStat;
                p.Ranks.NormalMileage = norStat.Mileage >= 1000 ? 1 : norStat.Mileage >= 500 ? 2 : 3;

                var attendance = p.Results.Count(x => x.Value.Speed >= p.Stats[Mode.Marathon].Record * 0.5) / (double)p.RacesCreated;
                p.Ranks.Attendance = attendance > 0.25 ? 1 : attendance > 0.2 ? 2 : attendance > 0.13 ? 3 : 4;

                p.Ranks.MarathonPeriodMileage = p.MarathonPeriodMileage >= p.Results.Count + 5 ? 1 : 2;

                ChangeProgress((int)(100.0 * ++counter / playersWithEnoughRacesBySpeed.Count));
            }

            playersWithEnoughRacesBySpeed.Sort((x, y) => -x.BestResultsAvgSpeed.Value.CompareTo(y.BestResultsAvgSpeed.Value));
            for (int i = 0; i < playersWithEnoughRacesBySpeed.Count / 2; i++)
                playersWithEnoughRacesBySpeed[i].Ranks.Speed = 1;


            playersWithEnoughRacesBySpeed.Sort((x, y) => x.BestResultsAvgErRate.Value.CompareTo(y.BestResultsAvgErRate.Value));
            for (int i = 0; i < playersWithEnoughRacesBySpeed.Count / 2; i++)
                playersWithEnoughRacesBySpeed[i].Ranks.ErRate = 1;

            foreach (var p in playersWithEnoughRacesBySpeed)
            {
                p.Ranks.WeightedTotal = Math.Round(
                      p.Ranks.Speed * 0.15
                    + p.Ranks.ErRate * 0.15
                    + p.Ranks.Attendance * 0.35
                    + p.Ranks.MarathonMileage * 0.15
                    + p.Ranks.MarathonPeriodMileage * 0.1
                    + p.Ranks.NormalMileage * 0.1, 5);
            }

            playersWithEnoughRacesBySpeed.Sort((x, y) =>
            {
                var result = x.Ranks.WeightedTotal.CompareTo(y.Ranks.WeightedTotal);
                if (result == 0) result = -x.Results.Count.CompareTo(y.Results.Count);
                if (result == 0) result = -x.BestResultsAvgSpeed.Value.CompareTo(y.BestResultsAvgSpeed.Value);
                return result;
            });

            for (int i = 0; i < playersWithEnoughRacesBySpeed.Count; i++)
            {
                var p = playersWithEnoughRacesBySpeed[i];
                p.TotalPlace = i + 1;

                if (i == 0 || p.Ranks.WeightedTotal != playersWithEnoughRacesBySpeed[i - 1].Ranks.WeightedTotal)
                {
                    p.Ranks.Total = i + 1;
                }
                else
                {
                    p.Ranks.Total = playersWithEnoughRacesBySpeed[i - 1].Ranks.Total;
                }
            }

            playersWithEnoughRacesBySpeed = playersWithEnoughRacesBySpeed.OrderBy(x => x.TotalPlace).ToList();
            for (int i = 0; i < playersWithEnoughRacesBySpeed.Count; i++)
            {
                var p = playersWithEnoughRacesBySpeed[i];
                if (lbrSettings.Scores?.Any() ?? false)
                {
                    if (i <lbrSettings.Scores.Length)
                        p.Prize = lbrSettings.Scores[i];

                    p.PrizeAndTypingPoints = p.Prize + p.TypingPoints;
                    p.TypingPointsRate = p.TypingPoints / p.Results.Count;
                    p.TotalPointsRate = p.PrizeAndTypingPoints / p.Results.Count;
                    p.TotalPointsRateCategory = p.TotalPointsRate >= 180 ? "В" : p.TotalPointsRate >= 150 ? "ВС" : p.TotalPointsRate >= 120 ? "С" : "М";
                }
            }
        }

        public List<int> GetBigRacePlayers()
        {
            List<int> result = new List<int>();
            string path = "Список участников БГ 2020.txt";

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach(var line in lines)
                {
                    var cells = line.Split('\t');
                    if (cells.Length >= 2)
                    {
                        if (int.TryParse(cells[1], out int id))
                            result.Add(id);
                    }
                }
            }
            return result;
        }

        private LBRSettings GetSettings()
        {
            LBRSettings result = new LBRSettings();
            string path = "LBR.settings.txt";
            var culture = new CultureInfo("ru-RU");

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                foreach(var line in lines)
                {
                    var cells = line.Split('=');
                    if (cells[0] == "StartDate" && DateTime.TryParse(cells[1], out var date))
                        result.StartDate = date;

                    if (cells[0] == "EndDate" && DateTime.TryParse(cells[1], culture, DateTimeStyles.None, out var endDate))
                        result.EndDate = endDate;

                    if (cells[0] == "Scores" && !string.IsNullOrEmpty(cells[1]))
                    {
                        result.Scores = cells[1].Split(',').Select(x => int.Parse(x.Replace(" ", ""))).ToArray();
                    }
                }
            }

            return result;
        }

        private class LBRSettings
        {
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int[] Scores { get; set; }
        }

    }
}
