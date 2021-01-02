using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Klavogonki
{
    [DataContract]
    public class Achievements
    {
        [DataMember(Name = "list")]
        public List<ProgressAchievement> List { get; set; }

        [DataMember(Name = "achieves")]
        public Dictionary<string, Achievement> Achieves { get; set; } // string == Guid


        [DataContract]
        public class ProgressAchievement
        {
            [DataMember(Name = "achieve_id")]
            public AchieveId AchieveId { get; set; }

            [DataMember(Name = "progress")]
            public double Progress { get; set; }

            [DataMember(Name = "level_progress")]
            public int? LevelProgress { get; set; }
        }

        [DataContract]
        public class Achievement
        {
            [DataMember(Name = "achieve")]
            public AchievmeneMode Achieve { get; set; }

            [DataMember(Name = "img")]
            public string Img { get; private set; } 

            [IgnoreDataMember]
            public int BookSnippet
            {
                get
                {
                    switch (Img)
                    {
                        case "Book100": return 300;
                        case "Book300": return 800;
                        case "Book600": return 1500;
                        case "Book1000": return 2200;
                        default: return 0;
                    }                    
                }
            }

        }

        [DataContract]
        public class AchievmeneMode
        {
            [DataMember(Name = "name")]
            public string Name { get; set; } //"Book", "NumRacesLocal"

            [DataMember(Name = "gametype")]
            public string GameType { get; set; } //при NumRacesLocal "voc-207"

            [DataMember(Name = "voc_id")]
            public int VocId { get; set; } //при Book 16510
        }

        [DataContract]
        public class AchieveId
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }
        }
    }
}
