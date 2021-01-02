using System;

namespace Klavogonki
{
    public class ResultParsed
    {
        public int Id { get; set; }
        public string Nick { get; set; }
        public int? RealPlace { get; set; }
        public TimeSpan? Time { get; set; }
        public int? Speed { get; set; }
        public double? ErRate { get; set; }
        public int? ErCnt { get; set; }
        public bool IsRecord { get; set; }
        public Rank Rank { get; set; }
        public Mode Mode { get; set; }
        public int Progress { get; set; }
        public bool HasLeftRace { get; set; }
        public int Mileage { get; set; }
        public bool NoErrorFail { get; set; }
        public int PointsIncrease { get; set; }
    }
}
