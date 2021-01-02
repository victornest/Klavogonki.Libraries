using System.Collections.Generic;

namespace Klavogonki
{
    public class CrashRaceSettings
    {
        public List<int> GroupsScores { get; set; } = new List<int> { 9, 8, 7 };

        public int NoErrorScores { get; set; } = 1;

        public List<string> PointsPresets { get; set; } = new List<string>
        {
            "5000, 4000, 3000, 2000, 1000, 900, 800, 700, 600, 500, 400, 300, 200, 100, 50",
            "6000, 5000, 4000, 3000, 2000, 1000, 800, 700, 600, 500",
            "5000, 4000, 3000, 2000, 1000"
        };

        public int PresetIndex { get; set; } = 1; // 1-based
        public int RacesCount { get; set; } = 20;
        public int MinimumScores { get; set; } = 100;
        public int BestInRankPrize { get; set; } = 1000;
        public int MasterPrize { get; set; } = 10000;
        public int MinimumNormalModeMileage { get; set; } = 1000;
        public int MinimumNoErrorModeMileage { get; set; } = 200;
        public int MinimumRacesForRecordPrize { get; set; } = 5;
    }
}
