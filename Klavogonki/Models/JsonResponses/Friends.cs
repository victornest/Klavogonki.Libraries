using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace Klavogonki
{
    [DataContract]
    public class Friends
    {
        [DataMember(Name = "users")]
        public List<Friend> Users { get; set; }

        [DataContract]
        public class Friend
        {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "login")]
            public string Login { get; set; }

            [DataMember(Name = "public_prefs")]
            public Public_prefs PublicPrefs { get; set; }
        }

        [DataContract]
        public class Public_prefs
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
    }
}
