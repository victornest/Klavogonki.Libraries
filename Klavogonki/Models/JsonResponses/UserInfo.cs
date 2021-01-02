using System;
using System.Runtime.Serialization;

namespace Klavogonki
{
    [Serializable]
    [DataContract]
    public class UserInfo
    {
        [IgnoreDataMember]
        public int Id { get; set; }

        [DataMember(Name = "ok")]
        public int Ok { get; set; }

        [DataMember(Name = "stats")]
        public UserStats Stats { get; set; }

        [Serializable]
        [DataContract]
        public class UserStats
        {

            [DataMember(Name = "registered")]
            public Registered Registered { get; set; }

            [DataMember(Name = "achieves_cnt")]
            public int AchievesCnt { get; set; }

            [DataMember(Name = "total_num_races")]
            public int TotalNumRaces { get; set; }

            [IgnoreDataMember]
            private int bestSpeed;

            [DataMember(Name = "best_speed")]
            public int? BestSpeed
            {
                get { return bestSpeed; }
                set { bestSpeed = value ?? 0; }
            }

            [DataMember(Name = "rating_level")]
            public int RatingLevel { get; set; }

            [DataMember(Name = "friends_cnt")]
            public int FriendsCnt { get; set; }

            [DataMember(Name = "vocs_cnt")]
            public int VocsCnt { get; set; }

            [DataMember(Name = "cars_cnt")]
            public int CarsCnt { get; set; }
            public UserStats()
            {
                Registered = new Registered();
            }
        }

        [Serializable]
        [DataContract]
        public class Registered
        {
            [IgnoreDataMember]
            public DateTime Date { get; private set; }

            [DataMember(Name = "usec")]
            public int Usec { get; set; }

            [IgnoreDataMember]
            private int sec;

            [DataMember(Name = "sec")]
            public int Sec
            {
                get => sec; 
                set
                {
                    sec = value;
                    Date = (new DateTime(1970, 1, 1).AddSeconds(value + 3 * 3600)).Date;
                }
            }
        }
    }
}
