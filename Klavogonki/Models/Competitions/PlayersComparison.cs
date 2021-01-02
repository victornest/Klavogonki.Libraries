using System;

namespace Klavogonki
{
    public class PlayersComparison
    {
        public Mode Mode { get; set; }
        public ComparisonStat Stat1 { get; set; }
        public ComparisonStat Stat2 { get; set; }
        public StatDifference Difference { get; set; }
    }

    public class ComparisonStat
    {
        public int? Record { get; set; }
        public int? AvgSpeed { get; set; }
        public double? AvgErRate { get; set; }
        public int? Mileage { get; set; }
    }

    public class StatDifference : IComparable
    {
        private readonly int recordDifference;
        private readonly int avgSpeedDifference;

        public StatDifference(int recordDifference, int avgSpeedDifference)
        {
            this.recordDifference = recordDifference;
            this.avgSpeedDifference = avgSpeedDifference;
        }

        public override string ToString()
        {
            return recordDifference + " (" + avgSpeedDifference + ")";
        }

        public int CompareTo(object sd)
        {
            return this.recordDifference - ((StatDifference)sd).recordDifference;
        }
    }

}
