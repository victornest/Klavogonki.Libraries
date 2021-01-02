using System.Collections.Generic;
using System.Linq;

namespace Klavogonki.Services.EventCalculators
{
    public class ProfsoyuzScoresCalculator : IScoresCalculator
    {
        public void CalculateScores(IEnumerable<Player> players)
        {
            if (players.Any())
            {
                var racesCreated = players.First().RacesCreated;

                for (var ri = 1; ri <= racesCreated; ri++)
                {
                    var pls = players.Where(x => x.Results.ContainsKey(ri)).ToList();
                    var racePlayers = pls.OrderBy(x => x.Results[ri].CalculatedPlace.Value).ToArray();

                    var lastPlace = racePlayers.Last().Results[ri].CalculatedPlace.Value;

                    foreach (var p in racePlayers)
                    {
                        var result = p.Results[ri];
                        result.Scores = GetScores(result.CalculatedPlace.Value, lastPlace);
                    }
                }
            }
        }

        public static int GetScores(int place, int lastPlace)
        {
            var placeFromEndZeroBased = lastPlace - place;
            var scores = 5 + placeFromEndZeroBased;

            if (place == 2 && place != lastPlace) scores += 1;
            else if (place == 1 && place != lastPlace) scores += 3;
            return scores;
        }
    }
}

//доехал 1 – 2 очка.
//если двое – 2 и 5 очков. 
//если трое – 2, 4, 7
//четверо – 2, 3, 5, 8