using System;
using System.Collections.Generic;
using System.Linq;
using Klavogonki;

namespace KlStatisticsLoader
{
    public class UserMigomanResult
    {
        public UserMigomanResult()
        {
            AllTimeMileages = new Dictionary<string, int>();
            BestTournamentSpeeds = new Dictionary<string, int>();
            TournamentMileages = new Dictionary<string, int>();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }

        public string UserHyperlink => $"=hyperlink(\"http://klavogonki.ru/u/#/{UserId}\";\"{UserName}\")";

        public Dictionary<string, int> BestTournamentSpeeds { get; private set; }

        public string GetTournamentMileageWithStatHyperlink(string gameType)
        {
            return GetTournamentValueWithHyperlink(gameType, TournamentMileages);
        }
        public string GetBestTournamentSpeedWithStatHyperlink(string gameType)
        {
            return GetTournamentValueWithHyperlink(gameType,BestTournamentSpeeds);
        }

        private string GetTournamentValueWithHyperlink(string gameType, Dictionary<string, int> values)
        {
            return GetValueWithStatHyperlink(gameType, values[gameType]);
        }

        public string GetValueWithStatHyperlink(string gameType, int value)
        {
            return $"=hyperlink(\"http://klavogonki.ru/u/#/{UserId}/stats/{gameType}\";\"{value}\")";
        }
        
        public int BestTournamentSpeedsTotal
        {
            get
            {
                return BestTournamentSpeeds.Sum(s => s.Value);
            }
        }

        public int BestAllTimeMainSpeed { get; set; }
        
        public Rank Rank => GetRank(BestAllTimeMainSpeed);

        public Dictionary<string, int> TournamentMileages { get; private set; } 
        public Dictionary<string, int> AllTimeMileages { get; private set; } // Before tournament? 

        public int GetCombinedAllTimeMileage(Dictionary<string, double> coeffs)
        {
            int combinedMileage = 0;
            foreach (var allTimeMileage in AllTimeMileages)
            {
                combinedMileage += (int)Math.Round(coeffs[allTimeMileage.Key] * allTimeMileage.Value);
            }

            return combinedMileage;
        }
        
        public static Rank GetRank(int speed)
        {
            if (speed < 100) return Rank.Novice;
            if (speed < 200) return Rank.Amateur;
            if (speed < 300) return Rank.TaxiDriver;
            if (speed < 400) return Rank.Pro;
            if (speed < 500) return Rank.Racer;
            if (speed < 600) return Rank.Maniac;
            if (speed < 700) return Rank.Superman;
            if (speed < 800) return Rank.CyberRacer;
            return Rank.ExtraCyber;
        }
    }
}