using System.Collections.Generic;
using System.Linq;

namespace Klavogonki
{
    public class RaceGroupResult
    {
        public RaceGroupResult()
        {
            Results = new Dictionary<int, Result>();
        }

        public RaceGroupResult(Dictionary<int, Result> results)
        {
            Results = results;
            Calculate(results);
        }

        public Dictionary<int, Result> Results { get; protected set; } // 1-based
        //public int RacesCount { get; private set; }

        public AverageType SpeedAverageType { get; set; }
        public double AvgSpeed => SpeedAverageType switch
        {
            AverageType.SymbolsWeighted => SymbolsWeightedAvgSpeed,
            AverageType.TimeWeighted => TimeWeightedAvgSpeed,
            _ => ArithmeticAvgSpeed
        };
        public double ArithmeticAvgSpeed { get; private set; }
        public double SymbolsWeightedAvgSpeed { get; private set; }
        public double TimeWeightedAvgSpeed { get; private set; }

        public int SpeedSum { get; private set; }
        public int MaxSpeed { get; private set; }
        public int MinSpeed { get; private set; }
        public int MaxSpeedNoerror { get; private set; }

        public AverageType ErRateAverageType { get; set; }
        public double AvgErRate => ErRateAverageType == AverageType.SymbolsWeighted ?
                                   SymbolsWeightedAvgErRate : ArithmeticAvgErRate;
        public double ArithmeticAvgErRate { get; private set; }
        public double SymbolsWeightedAvgErRate { get; private set; }

        public double ErRateSum { get; private set; }
        public double MaxErRate { get; private set; }
        public double MinErRate { get; private set; }


        public double AvgErCnt { get; private set; }
        public int ErCntSum { get; private set; }
        public int MaxErCnt { get; private set; }
        public int MinErCnt { get; private set; }

        public double Scores { get; set; }    
        public double Scores2 { get; set; }

        public bool IsBestScores { get; set; }
        public bool IsBestSpeed { get; set; }

        protected void Calculate(Dictionary<int, Result> results)
        {
            if (results.Any())
            {
                Results = results;

                ArithmeticAvgSpeed = results.Average(x => x.Value.Speed);
                SymbolsWeightedAvgSpeed = results.Sum(x => x.Value.Speed * x.Value.SymbolsCnt)
                    / results.Sum(x => x.Value.SymbolsCnt);
                TimeWeightedAvgSpeed = results.Sum(x => x.Value.Speed * x.Value.Time.TotalMinutes)
                    / results.Sum(x => x.Value.Time.TotalMinutes);
                SpeedSum = results.Sum(x => x.Value.Speed);
                MaxSpeed = results.Max(x => x.Value.Speed);
                MinSpeed = results.Min(x => x.Value.Speed);

                ArithmeticAvgErRate = results.Average(x => x.Value.ErRate);
                SymbolsWeightedAvgErRate = results.Sum(x => x.Value.ErRate * x.Value.SymbolsCnt) / results.Sum(x => x.Value.SymbolsCnt);
                ErRateSum = results.Sum(x => x.Value.ErRate);
                MaxErRate = results.Max(x => x.Value.ErRate);
                MinErRate = results.Min(x => x.Value.ErRate);

                AvgErCnt = results.Average(x => x.Value.ErCnt);
                ErCntSum = results.Sum(x => x.Value.ErCnt);
                MaxErCnt = results.Max(x => x.Value.ErCnt);
                MinErCnt = results.Min(x => x.Value.ErCnt);

                Scores = results.Sum(x => x.Value.Scores);
            }
        }
    }
}
