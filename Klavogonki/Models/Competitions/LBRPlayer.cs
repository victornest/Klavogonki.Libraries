using System.Collections.Generic;

namespace Klavogonki
{
    public class LBRPlayer : Player
    {
        public Dictionary<int, Result> BestResults { get; set; } = new Dictionary<int, Result>();
        public double? BestResultsAvgSpeed { get; set; }
        public double? BestResultsAvgErRate { get; set; }
        public bool HasEnoughRacesBySpeed { get; set; }
        public LBRRanks Ranks { get; set; }
        public int TypingPoints { get; set; }
        public int MarathonPeriodMileage { get; set; } = -1;

        public int? TotalPlace { get; set; }
        public int Prize { get; set; }

        public int PrizeAndTypingPoints { get; set; }
        public int TypingPointsRate { get; set; }
        public int TotalPointsRate { get; set; }
        public string TotalPointsRateCategory { get; set; }


        public class LBRRanks
        {
            public int Speed { get; set; }
            public int ErRate { get; set; }
            public int Attendance { get; set; }
            public int MarathonMileage { get; set; }
            public int MarathonPeriodMileage { get; set; }
            public int NormalMileage { get; set; }

            public double WeightedTotal { get; set; }
            public int Total { get; set; }
        }
    }
}
