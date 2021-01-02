using System;
using System.Collections.Generic;
using System.Linq;

namespace Klavogonki
{
    public class Player : RaceGroupResult, IComparable<Player>
    {
        public Dictionary<int, RaceGroupResult> ModeResults { get; } = new Dictionary<int, RaceGroupResult>(); // 1-based

        public int Id { get; set; }
        public string Nick { get; set; }
        public Rank Rank { get; set; }

        public int RacesCreated { get; set; }

        public bool HasEnoughRaces { get; set; } = true; //достаточно ли доездов для зачета
        public bool HasEnoughMileage { get; set; } = true; //есть ли достаточный пробег для награждения
        public bool IsExcluded { get; set; }
        public bool IsSuccessful => HasEnoughMileage && HasEnoughRaces && !IsExcluded;



        public TimeSpan AvgTime { get; private set; }
        public double AvgRealPlace { get; private set; }
        public double? AvgCalculatedPlace { get; private set; }
        public double SymbolsSum { get; private set; }

        public bool BestInRankBySpeedTotal { get; set; } //лучший в ранге из всех игроков

        public int[] Prizes { get; set; } = new int[6]; //награждение по зачетам
        public int TotalPrizeSum => Prizes?.Sum() ?? 0; //награждение, клавогоночные очки
        public string SendingPrizeMessage { get; set; } //сообщение при передаче очков
        public string ForumMessage { get; set; } //сообщение о награждении для форума
        public List<string> BortjournalMessage { get; set; } // запись в бортжурнал

        public bool IsCalculated { get; private set; }

        public Dictionary<string, QuickStat> Stats { get; } = new Dictionary<string, QuickStat>();

 
        public void AddParsedResult(int raceIndex, ResultParsed parsedResult, int scores = 0)
        {
            Result result = new Result(parsedResult) { Scores = scores };
            Results.Add(raceIndex, result);
            this.Rank = parsedResult.Rank;
            this.Nick = parsedResult.Nick;
            HasEnoughRaces = true;

            IsCalculated = false;
        }

        public void Calculate()
        {         
            if (Results.Any())
            {
                base.Calculate(Results);

                SymbolsSum = Results.Sum(x => x.Value.SymbolsCnt);
                AvgTime = TimeSpan.FromSeconds(Results.Average(x => x.Value.Time.TotalSeconds));
                AvgRealPlace = Results.Average(x => x.Value.RealPlace);

                var notNullCalcPlaceResults = Results.Where(x => x.Value.CalculatedPlace.HasValue);
                if (notNullCalcPlaceResults.Any())
                    AvgCalculatedPlace = notNullCalcPlaceResults.Average(x => x.Value.CalculatedPlace.Value);
            }

            for (int i = 1; i <= RacesCreated; i++)
            {
                if (!base.Results.ContainsKey(i)) continue;
                if (base.Results[i].Speed == MaxSpeed) base.Results[i].IsBestSpeed = true;
                else if (base.Results[i].Speed == MinSpeed)
                {
                    base.Results[i].IsWorstSpeed = true;
                }
                if (base.Results[i].ErRate == MinErRate) base.Results[i].IsBestErRate = true;
                else if (base.Results[i].ErRate == MaxErRate)
                {
                    base.Results[i].IsWorstErRate = true;
                }
                if (base.Results[i].ErCnt == MinErCnt) base.Results[i].IsBestErCnt = true;
                else if (base.Results[i].ErCnt == MaxErCnt)
                {
                    base.Results[i].IsWorstErCnt = true;
                }

                base.Results[i].RelativeSpeed = base.Results[i].Speed / AvgSpeed;
            }

            IsCalculated = true;
        }

        public void CalculateGroups(List<GroupInfo> groups)
        {
            int modeCounter = 1;
            foreach (var group in groups)
            {
                var groupResults = Results.Where(x => x.Key >= group.StartPosition && x.Key <= group.StartPosition + group.Length - 1).ToDictionary(x => x.Key, x => x.Value);

                if (groupResults.Any())
                {
                    var modeResult = new RaceGroupResult(groupResults);
                    ModeResults.Add(modeCounter, modeResult);
                }
                modeCounter++;
            }
        }


        public int CompareTo(Player p)
        {
            int result = -this.HasEnoughRaces.CompareTo(p.HasEnoughRaces);
            if (result == 0) result = -this.Scores.CompareTo(p.Scores);
            if (result == 0) result = -this.AvgSpeed.CompareTo(p.AvgSpeed);
            if (result == 0) result = this.AvgErRate.CompareTo(p.AvgErRate);
            return result;
        }

        public string GetBBLink(bool needRankColor = false)
        {
            if (needRankColor)
                return $"[url=\"http://klavogonki.ru/profile/{Id}\"][color=\"#{Rank.HexColor}\"]{Nick}[/color][/url]";
            else return $"[url=\"http://klavogonki.ru/profile/{Id}\"]{Nick}[/url]";
        }

    }

}
