using System.Runtime.Serialization;

namespace Klavogonki
{
    [DataContract]
    public class UserSummary
    {
        [DataMember(Name = "user")]
        public UserSum User { get; set; }

        [DataMember(Name = "is_online")]
        public bool IsOnline { get; set; }

        [DataMember(Name = "car")]
        public CarData Car { get; set; }

        [DataMember(Name = "level")]
        public int Level { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "blocked")]
        public int Blocked { get; set; }

        [DataContract]
        public class UserSum
        {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "login")]
            public string Login { get; set; }

            [DataMember(Name = "public_prefs")]
            public PublicPrefs Public_prefs { get; set; }
        }

        [DataContract]
        public class PublicPrefs
        {
            [DataMember(Name = "journal")]
            public Journal Journal { get; set; }

            [DataMember(Name = "allow_friend_invite")]
            public string AllowFriendInvite { get; set; }

            [DataMember(Name = "allow_message")]
            public string AllowMessage { get; set; }

            [DataMember(Name = "stats")]
            public Stats Stats { get; set; }
        }

        [DataContract]
        public class Journal
        {
            [DataMember(Name = "allow_post")]
            public string AllowPost { get; set; }

            [DataMember(Name = "allow_comment")]
            public string AllowComment { get; set; }
        }

        [DataContract]
        public class Stats
        {
            [DataMember(Name = "allow_view")]
            public string AllowView { get; set; }

            [DataMember(Name = "allow_view_data")]
            public string AllowViewData { get; set; }

            [DataMember(Name = "allow_view_detailed")]
            public string AllowViewDetailed { get; set; }

            [DataMember(Name = "allow_download_tsf")]
            public string AllowDownloadTsf { get; set; }

        }

        [DataContract]
        public class CarData
        {
            [DataMember(Name = "car")]
            public int Car { get; set; }

            [DataMember(Name = "color")]
            public string Color { get; set; }
   

            [IgnoreDataMember]
            public int[] Tuning { get; set; }

            [IgnoreDataMember]
            public string AeroUrl { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }
    }
}
