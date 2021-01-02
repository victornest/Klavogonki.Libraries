namespace Klavogonki
{
    public class Stat
    {
        public Stat(int userId)
        {
            UserId = userId;
        }
        public Stat(QuickStat quickStat)
        {
            QuickStat = quickStat;
            UserId = quickStat.Id;
        }

        public int UserId { get; private set; }
        public QuickStat QuickStat { get; set; }

        public PeriodStat PeriodStat { get; set; }

        public int? TopResult { get; set; }
        public bool? HasPremium { get; set; }
    }
}
