namespace Klavogonki
{
    public class PeriodStat
    {
        public int Mileage { get; set; }

        public int? MaxSpeed { get; set; }

        public int[] BestResultsBelowMaxSpeed { get; set; }
        public int[] RawBestResults { get; set; }
    }
}
