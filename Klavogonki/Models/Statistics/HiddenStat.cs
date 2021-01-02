using System.Collections.Generic;

namespace Klavogonki
{
    public class HiddenStat
    {
        public HiddenStat(int userId)
        {
            UserId = userId;
        }

        public int UserId { get; private set; }

        public List<QuickStat> QuickStats { get; private set; } = new List<QuickStat>();   

        public int TotalMileage { get; set; }
        public int MileageCount { get; set; }
        public double MileagePercent { get => TotalMileage == 0 ? 0 : (double)MileageCount / TotalMileage; }
    }
}
