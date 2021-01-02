using System;
using System.Collections.Generic;

namespace Klavogonki
{
    public class SuccessParamSpeedComparer : IComparer<Player>
    {
        private Func<Player, double> paramFunc;
        private bool isDescending;
        public SuccessParamSpeedComparer(Func<Player, double> paramFunc, bool isDescending = true)
        {
            this.paramFunc = paramFunc;
            this.isDescending = isDescending;
        }

        public int Compare(Player p1, Player p2)
        {
            int result = -p1.IsSuccessful.CompareTo(p2.IsSuccessful);
            if (result == 0) result = (isDescending ? -1 : 1) * paramFunc(p1).CompareTo(paramFunc(p2));
            if (result == 0) result = -p1.AvgSpeed.CompareTo(p2.AvgSpeed);
            return result;
        }
    }

    public class SpeedComparer : IComparer<Player>
    {
        public int Compare(Player p1, Player p2)
        {
            int result = -p1.IsSuccessful.CompareTo(p2.IsSuccessful);
            if (result == 0) result = -p1.AvgSpeed.CompareTo(p2.AvgSpeed);
            return result;
        }
    }

    public class ErrorRateComparer : IComparer<Player>
    {
        public int Compare(Player p1, Player p2)
        {
            int result = -p1.IsSuccessful.CompareTo(p2.IsSuccessful);
            if (result == 0) result = p1.AvgErRate.CompareTo(p2.AvgErRate);
            return result;
        }
    }

    public class ErrorCountComparer : IComparer<Player>
    {
        public int Compare(Player p1, Player p2)
        {
            int result = -p1.IsSuccessful.CompareTo(p2.IsSuccessful);
            if (result == 0) result = p1.ErCntSum.CompareTo(p2.ErCntSum);
            return result;
        }
    }    

    public class ScoresAndSpeedComparer : IComparer<Player>
    {
        public int Compare(Player p1, Player p2)
        {
            int result = -p1.IsSuccessful.CompareTo(p2.IsSuccessful);
            if (result == 0) result = -p1.Scores.CompareTo(p2.Scores);
            if (result == 0) result = -p1.AvgSpeed.CompareTo(p2.AvgSpeed);
            return result;
        }
    }

    public class CrashRaceComparer : IComparer<Player>
    {
        public int Compare(Player p1, Player p2)
        {
            int result = -p1.HasEnoughMileage.CompareTo(p2.HasEnoughMileage);
            if (result == 0) result = -p1.Scores.CompareTo(p2.Scores);
            if (result == 0) result = -p1.Results.Count.CompareTo(p2.Results.Count);
            if (result == 0) result = p1.ErCntSum.CompareTo(p2.ErCntSum);
            if (result == 0) result = -p1.AvgSpeed.CompareTo(p2.AvgSpeed);
            return result;
        }
    }

     
    public class KokComparer : IComparer<Player>
    {
        public int Compare(Player p1, Player p2)
        {
            int result = p1.HasEnoughRaces.CompareTo(p2.HasEnoughRaces);
            if (result == 0) result = -p1.Scores.CompareTo(p2.Scores);
            if (result == 0) result = -p1.Results.Count.CompareTo(p2.Results.Count);
            if (result == 0) result = -p1.MaxSpeedNoerror.CompareTo(p2.MaxSpeedNoerror);
            if (result == 0) result = -p1.MaxSpeed.CompareTo(p2.MaxSpeed);
            return result;
        }
    }

    public class RallyCrossComparer : IComparer<Player>
    {
        public int Compare(Player p1, Player p2)
        {
            int result = -p1.Scores.CompareTo(p2.Scores);
            if (result == 0) result = p1.ErCntSum.CompareTo(p2.ErCntSum);
            if (result == 0) result = -p1.MinSpeed.CompareTo(p2.MinSpeed);
            return result;
        }
    }


    public class ProfsoyuzComparer : IComparer<Player>
    {
        public int Compare(Player p1, Player p2)
        {
            int result = -p1.IsSuccessful.CompareTo(p2.IsSuccessful);
            if (result == 0) result = -p1.Scores.CompareTo(p2.Scores);
            if (result == 0) result = -p1.Results.Count.CompareTo(p2.Results.Count);
            if (result == 0) result = -p1.AvgSpeed.CompareTo(p2.AvgSpeed);
            return result;
        }
    }

}
