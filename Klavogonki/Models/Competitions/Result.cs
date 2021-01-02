using System;

namespace Klavogonki
{
    public class Result : ResultParsed
    {
        public Result(ResultParsed resultParsed)
        {
            Id = resultParsed.Id;
            Nick = resultParsed.Nick;
            RealPlace = resultParsed.RealPlace.Value;
            Time = resultParsed.Time.Value;
            Speed = resultParsed.Speed.Value;
            ErRate = resultParsed.ErRate.Value;
            ErCnt = resultParsed.ErCnt.Value;
            IsRecord = resultParsed.IsRecord;
            Rank = resultParsed.Rank;
            Mode = resultParsed.Mode;
            Progress = resultParsed.Progress;
            HasLeftRace = resultParsed.HasLeftRace;
            Mileage = resultParsed.Mileage;
            NoErrorFail = resultParsed.NoErrorFail;
            PointsIncrease = resultParsed.PointsIncrease;
            SymbolsCnt = Speed * Time.TotalMinutes;
        }

        public new int RealPlace { get; private set; }
        public new TimeSpan Time { get; private set; }
        public new int Speed { get; private set; }
        public new double ErRate { get; private set; }
        public new int ErCnt { get; private set; }
        public double SymbolsCnt { get; private set; }

        public bool IsBestSpeed { get; set; }
        public bool IsWorstSpeed { get; set; }

        public bool IsBestErRate { get; set; }
        public bool IsWorstErRate { get; set; }

        public bool IsBestErCnt { get; set; }
        public bool IsWorstErCnt { get; set; }     
        
        public bool IsSeries { get; set; }        
        public int Scores { get; set; }
        public double? RelativeSpeed { get; set; }
        public int? CalculatedPlace { get; set; }
    }
}
