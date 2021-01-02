using System;
using System.Collections.Generic;
namespace Klavogonki
{
    public class ModeStatSettings
    {
        public List<int> UserIds { get; set; }
        public List<string> ModeIds { get; set; }

        public int MaxUsers { get; set; }
        public bool NeedQuickStat { get; set; }
        public bool NeedPeriodStat { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
