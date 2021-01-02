using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Klavogonki
{
    /// <summary>
    /// для десер. статистики по неделям
    /// </summary>
    public class WeeksStatResponse
    {
        [DataMember(Name = "ok")]
        public int Ok { get; set; }

        [DataMember(Name = "list")]
        public List<WeekResult> List { get; set; }

        public class WeekResult
        {
            [DataMember(Name = "min_date")]
            public Date2 Min_date { get; set; }

            [DataMember(Name = "max_date")]
            public Date2 Max_date { get; set; }

            [DataMember(Name = "min_speed")]
            public int Min_speed { get; set; }

            [DataMember(Name = "avg_speed")]
            public double Avg_speed { get; set; }

            [DataMember(Name = "max_speed")]
            public int Max_speed { get; set; }

            [DataMember(Name = "min_errors")]
            public double Min_errors { get; set; }

            [DataMember(Name = "avg_errors")]
            public double Avg_errors { get; set; }

            [DataMember(Name = "max_errors")]
            public double Max_errors { get; set; }
            
            [DataMember(Name = "cnt")]
            public int Cnt { get; set; } //количество заездов

            [DataMember(Name = "avg_speed_end")]
            public double Avg_speed_end { get; set; }

            [DataMember(Name = "avg_errors_end")]
            public double Avg_errors_end { get; set; }
        }
        public class Date2
        {
            [DataMember(Name = "usec")]
            public int Usec { get; set; }

            [DataMember(Name = "date")]
            public DateTime Date { get; private set; }

            private int sec;

            [DataMember(Name = "sec")]
            public int Sec
            {
                get { return sec; }
                set
                {
                    sec = value;
                    Date = new DateTime(1970, 1, 1).AddSeconds(value + 14400);
                }
            }
        }
    }
}
