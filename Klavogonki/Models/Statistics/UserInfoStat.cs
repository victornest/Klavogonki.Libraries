using System;

namespace Klavogonki
{    
    [Serializable]
    public class UserInfoStat
    {
        public UserInfoStat()
        {
            Ranks = new UserRanks();
        }

        public UserInfo UserInfo { get; set; }

        public QuickStat QuickStat { get; set; }

        public UserRanks Ranks { get; set; }

        public int? FriendsRating { get; set; }


        public void CalculateRanks()
        {
            var id = UserInfo.Id;
            var stats = UserInfo.Stats;

            int bId; //б.дата рег
            if (id > 500000) bId = 0;
            else if (id > 450000) bId = 1;
            else if (id > 400000) bId = 2;
            else if (id > 350000) bId = 3;
            else if (id > 300000) bId = 4;
            else if (id > 250000) bId = 5;
            else if (id > 200000) bId = 6;
            else if (id > 150000) bId = 7;
            else if (id > 100000) bId = 8;
            else if (id > 50000) bId = 9;
            else bId = 10;
            Ranks.Id = bId;

            if (stats != null)
            {

                int bRecord; //б.рекорд
                if (stats.BestSpeed <= 175) bRecord = 0;
                else if (stats.BestSpeed < 250) bRecord = 1;
                else if (stats.BestSpeed < 325) bRecord = 2;
                else if (stats.BestSpeed < 400) bRecord = 3;
                else if (stats.BestSpeed < 475) bRecord = 4;
                else if (stats.BestSpeed < 550) bRecord = 5;
                else if (stats.BestSpeed < 625) bRecord = 6;
                else if (stats.BestSpeed < 700) bRecord = 7;
                else if (stats.BestSpeed < 775) bRecord = 8;
                else if (stats.BestSpeed < 850) bRecord = 9;
                else bRecord = 10;
                Ranks.Record = bRecord;
                //r.record = (int)(stats.BestSpeed * 1.15 / 100 - 0.5); //б.рек

                int bMileage; //б.пробег
                if (stats.TotalNumRaces <= 500) bMileage = 0;
                else if (stats.TotalNumRaces <= 1000) bMileage = 1;
                else if (stats.TotalNumRaces <= 2500) bMileage = 2;
                else if (stats.TotalNumRaces <= 5000) bMileage = 3;
                else if (stats.TotalNumRaces <= 10000) bMileage = 4;
                else if (stats.TotalNumRaces <= 20000) bMileage = 5;
                else if (stats.TotalNumRaces <= 35000) bMileage = 6;
                else if (stats.TotalNumRaces <= 55000) bMileage = 7;
                else if (stats.TotalNumRaces <= 75000) bMileage = 8;
                else if (stats.TotalNumRaces <= 100000) bMileage = 9;
                else bMileage = 10;
                Ranks.Mileage = bMileage;


                int bRatLevel; //б.рейт.ур
                if (stats.RatingLevel <= 5) bRatLevel = 0;
                else if (stats.RatingLevel <= 10) bRatLevel = 1;
                else if (stats.RatingLevel <= 15) bRatLevel = 2;
                else if (stats.RatingLevel <= 20) bRatLevel = 3;
                else if (stats.RatingLevel <= 25) bRatLevel = 4;
                else if (stats.RatingLevel <= 30) bRatLevel = 5;
                else if (stats.RatingLevel <= 40) bRatLevel = 6;
                else if (stats.RatingLevel <= 50) bRatLevel = 7;
                else if (stats.RatingLevel <= 60) bRatLevel = 8;
                else if (stats.RatingLevel <= 70) bRatLevel = 9;
                else bRatLevel = 10;
                Ranks.Rating = bRatLevel;

                int bFriends; //б.друзей
                if (stats.FriendsCnt < 5) bFriends = 0;
                else if (stats.FriendsCnt < 10) bFriends = 1;
                else if (stats.FriendsCnt < 17) bFriends = 2;
                else if (stats.FriendsCnt < 25) bFriends = 3;
                else if (stats.FriendsCnt < 40) bFriends = 4;
                else if (stats.FriendsCnt < 60) bFriends = 5;
                else if (stats.FriendsCnt < 100) bFriends = 6;
                else if (stats.FriendsCnt < 150) bFriends = 7;
                else if (stats.FriendsCnt < 200) bFriends = 8;
                else if (stats.FriendsCnt < 250) bFriends = 9;
                else bFriends = 10;
                Ranks.Friends = bFriends;
            }
            Ranks.Total = (int)((1.5 * Ranks.Record + 2 * Ranks.Mileage + 1.5 * Ranks.Id + Ranks.Rating + Ranks.Friends) / 0.7);
        }

        [Serializable]
        public class UserRanks
        {
            public int Id { get; set; }
            public int Record { get; set; }
            public int Mileage { get; set; }
            public int Rating { get; set; }
            public int Friends { get; set; }
            public int Total { get; set; }
        }
    }
}
