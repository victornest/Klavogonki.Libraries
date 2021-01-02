using System;
using System.Collections.Generic;

namespace Klavogonki
{
    public class LBRComparer : IComparer<LBRPlayer>
    {
        public int Compare(LBRPlayer p1, LBRPlayer p2)
        {
            int result = -p1.HasEnoughRacesBySpeed.CompareTo(p2.HasEnoughRacesBySpeed);

            if (result == 0) 
                result = -p1.BestResultsAvgSpeed.HasValue.CompareTo(p2.BestResultsAvgSpeed.HasValue);

            if (result == 0 && p1.BestResultsAvgSpeed.HasValue)
                result = -p1.BestResultsAvgSpeed.Value.CompareTo(p2.BestResultsAvgSpeed.Value);

            if (result == 0)
                result = -p1.Results.Count.CompareTo(p2.Results.Count);

            if (result == 0) 
                result = -p1.AvgSpeed.CompareTo(p2.AvgSpeed);
            return result;
        }
    }
    public class LBRPlaceComparer : IComparer<LBRPlayer>
    {
        public int Compare(LBRPlayer p1, LBRPlayer p2)
        {
            int result = -p1.HasEnoughRacesBySpeed.CompareTo(p2.HasEnoughRacesBySpeed);
            if (result == 0) 
                result = -p1.TotalPlace.HasValue.CompareTo(p2.TotalPlace.HasValue);
            if (result == 0 && p1.TotalPlace.HasValue)
                result = p1.TotalPlace.Value.CompareTo(p2.TotalPlace.Value);
            return result;
        }
    }
}
