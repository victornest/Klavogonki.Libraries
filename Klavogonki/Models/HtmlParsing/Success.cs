using System;
using System.Collections.Generic;

namespace Klavogonki
{
    [Serializable]
    public class Success
    {
        public int Place { get; set; }

        public int Id { get; set; }

        public string Nick { get; set; }

        public int Progress { get; set; }

        public int ProgressPercent { get; set; }

        public int AvgSpeed { get; set; }

        public int TotalMileage { get; set; }

        public int MileageIncrease { get; set; }

        public bool IsNew { get; set; }
    }

    [Serializable]
    public class Successes : List<Success>
    {
        public DateTime DateTime { get; set; }
    }
}