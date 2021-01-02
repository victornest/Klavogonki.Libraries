using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Klavogonki
{
    [Serializable]
    public class QuickStat
    {
        public int Id { get; set; }
        public string Nick { get; set; }
        public Rank Rank { get; set; }

        public int Level { get; set; }
        public int TotalMileage { get; set; }

        public Mode Mode { get; set; }
        public int Record { get; set; }
        public int AvgSpeed { get; set; }
        public double AvgErRate { get; set; }
        public int Mileage { get; set; }
        public TimeSpan Time { get; set; }
        public int? DayTop { get; set; }
        public int? WeekTop { get; set; }
        public int? BookTop { get; set; }

        public List<Award> Awards { get; set; }  = new List<Award>();
        public int BooksCount { get => BooksGold + BooksSilver + BooksBronze; }
        public int BooksGold { get; set; }
        public int BooksSilver { get; set; }
        public int BooksBronze { get; set; }


        public int UpdateRecordAndRank(int newRecord)
        {
            int difference = newRecord - this.Record;
            this.Record = newRecord;
            this.Rank = Rank.GetByRecord(newRecord);
            return difference;
        }

        public class QuickStartMileageComparer : IComparer<QuickStat>
        {
            public int Compare(QuickStat x, QuickStat y)
            {
                return y.Mileage - x.Mileage;
            }

        }

        public class QuickStartRecordComparer : IComparer<QuickStat>
        {
            public int Compare(QuickStat x, QuickStat y)
            {
                return y.Record - x.Record;
            }

        }
    }
}
