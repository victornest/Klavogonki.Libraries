using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Klavogonki
{
    /// <summary>
    /// для получения открытой статистики по всем режимам
    /// </summary>
    [DataContract]
    public class OpenStat
    {
        [DataMember(Name = "ok")]
        public int Ok { get; set; }

        [DataMember(Name = "gametypes")]
        public Dictionary<string, Mode> Gametypes { get; set; }

        [DataMember(Name = "recent_gametypes")]
        public List<string> RecentGametypes { get; set; }

        [DataContract]
        public class Mode
        {
            [DataMember(Name = "id")]
            public int Id { get; set; } //id только для словаря

            [DataMember(Name = "name")]
            public string Name { get; set; } //название на русском

            [DataMember(Name = "type")]
            public string Type { get; set; }

            [DataMember(Name = "symbols")]
            public int Symbols { get; set; }

            [DataMember(Name = "rows")]
            public int Rows { get; set; }

            [DataMember(Name = "num_races")]
            public int NumRaces { get; set; }

            [DataMember(Name = "info")]
            public Info Info { get; set; }
        }

        [DataContract]
        public class Info
        {
            [DataMember(Name = "id")]
            public int Id { get; set; } //непонятно что

            [DataMember(Name = "user_id")]
            public int UserId { get; set; }

            [DataMember(Name = "mode")]
            public string Mode { get; set; }

            [DataMember(Name = "texttype")]
            public int TextType { get; set; }                      

            [DataMember(Name = "num_races")]
            public int NumRaces { get; set; }

            //http://klavogonki.ru/api/profile/get-stats-overview?userId=215941 игрок с null в avg_speed, best_speed, avg_error

            [IgnoreDataMember]
            private double avgSpeed;

            [DataMember(Name = "avg_speed")]
            public double? AvgSpeed
            {
                get => avgSpeed;
                set => avgSpeed = value ?? 0;
            }

            [IgnoreDataMember]
            private int bestSpeed;

            [DataMember(Name = "best_speed")]
            public int? BestSpeed
            {
                get => bestSpeed;
                set => bestSpeed = value ?? 0;
            }

            [IgnoreDataMember]
            private double avgError;

            [DataMember(Name = "avg_error")]
            public double? AvgError 
            { 
                get => avgError; 
                set => avgError = value ?? 0 / 100; 
            } 

            [IgnoreDataMember]
            public TimeSpan Time { get => TimeSpan.FromSeconds(Haul); }

            [DataMember(Name = "haul")]
            public int Haul { get; set; }

            [DataMember(Name = "qual")]
            public int Qual { get; set; } //nullable?

            [DataMember(Name = "dirty")]
            public int Dirty { get; set; }
        }
    }
}
