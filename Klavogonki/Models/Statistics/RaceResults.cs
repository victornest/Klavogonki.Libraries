using System.Collections.Generic;

namespace Klavogonki
{
    public class RaceResults : List<DayRacesResponse.RaceResult>
    {
        public bool HasPremium { get; set; }
    }
}
