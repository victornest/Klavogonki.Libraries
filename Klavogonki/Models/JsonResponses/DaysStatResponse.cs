using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Klavogonki
{
    /// <summary>
    /// для десер. статистики по дням
    /// </summary>
    [DataContract]
    public class DaysStatResponse
    {
        [DataMember(Name = "ok")]
        public int Ok { get; set; }

        [DataMember(Name = "list")]
        public List<DayResult> List { get; set; }

        [DataContract]
        public class DayResult
        {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "user_id")]
            public int UserId { get; set; }

            [DataMember(Name = "date")]
            public Date1 Date { get; set; }

            [DataMember(Name = "mode")]
            public string Mode { get; set; }

            [DataMember(Name = "texttype")]
            public int TextType { get; set; }

            [DataMember(Name = "min_speed")]
            public double MinSpeed { get; set; }

            [DataMember(Name = "avg_speed")]
            public double AvgSpeed { get; set; }

            [DataMember(Name = "avg_speed_end")]
            public double AvgSpeedEnd { get; set; }

            [DataMember(Name = "max_speed")]
            public int MaxSpeed { get; set; }

            [DataMember(Name = "min_errors")]
            public double MinErrors { get; set; }

            [DataMember(Name = "avg_errors")]
            public double AvgErrors { get; set; }

            [DataMember(Name = "avg_errors_end")]
            public double AvgErrorsEnd { get; set; }

            [DataMember(Name = "max_errors")]
            public double MaxErrors { get; set; }

            [DataMember(Name = "cnt")]
            public int Cnt { get; set; }

            [DataMember(Name = "min_date")]
            public Date2 MinDate { get; set; }

            [DataContract]
            public class Date1
            {
                [DataMember(Name = "sec")]
                public int Sec { get; set; }

                [DataMember(Name = "usec")]
                public int Usec { get; set; }
            }
        }

        [DataContract]
        public class Date2
        {
            [DataMember(Name = "usec")]
            public int Usec { get; set; }

            [IgnoreDataMember]
            public DateTime Date { get; private set; }

            [IgnoreDataMember]
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
