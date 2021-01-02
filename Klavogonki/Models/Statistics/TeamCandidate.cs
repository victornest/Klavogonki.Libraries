using System;

namespace Klavogonki
{
    public class TeamCandidate
    {
        public int Id { get; set; }
        public string Nick { get; set; }
        public int LeadFriendsCount { get; set; }
        public bool HasTwoThirdEhoughMileage { get; set; }
        public bool HasEhoughMileage { get; set; }
        public bool IsTeamPlayer { get; set; }
        public bool IsOldTeamPlayer { get; set; }
        public int Scores { get; set; }

        public TopStat TopStat { get; set; }

        public QuickStat QuickStat { get; set; }

        public int ThreeMonthsMileage { get; set; }
        public int OneMonthMileage { get; set; }
        public double SaturdaysMileage { get; set; }
        public double SaturdaysMileageRate => ThreeMonthsMileage == 0 ? 0 : SaturdaysMileage / ThreeMonthsMileage;
        public int[] Best5Results { get; set; }
    }
}
