using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Klavogonki
{
    /// <summary>
    /// для десер. подробной статистики за день
    /// </summary>
    [DataContract]
    public class DayRacesResponse
    {
        [DataMember(Name = "ok")]
        public int Ok { get; set; }

        [DataMember(Name = "list")]
        public List<RaceResult> List { get; set; }

        [DataContract]
        public class RaceResult
        {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "date")]
            public Date2 Date { get; set; }

            [DataMember(Name = "time")]
            public int Time { get; set; }

            [DataMember(Name = "speed")]
            public int Speed { get; set; }

            [DataMember(Name = "errors")]
            public int Errors { get; set; }

            [IgnoreDataMember]
            private double errorRate;

            [DataMember(Name = "error_rate")]
            public double ErrorRate { get => errorRate; set => errorRate = value / 100; }        
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
