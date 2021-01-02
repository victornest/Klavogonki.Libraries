using System;

namespace Klavogonki
{
    public class DayStat
    {
        public DateTime Date { get; set; }

        public int Mileage { get; set; }

        public double MinSpeed { get; set; }

        public double AvgSpeed { get; set; }

        public double AvgSpeedEnd { get; set; }

        public int MaxSpeed { get; set; }

        public double MinErrors { get; set; }

        public double AvgErrors { get; set; }

        public double AvgErrorsEnd { get; set; }

        public double MaxErrors { get; set; }
    }
}
