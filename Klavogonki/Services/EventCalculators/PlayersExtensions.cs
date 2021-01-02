using System.Collections.Generic;
using System.Linq;

namespace Klavogonki
{
    public static class PlayersExtensions
    {
        public static void Calculate(this IEnumerable<Player> players)
        {
            foreach (var player in players)
            {
                player.Calculate();
            }
        }

        public static void CalculateGroups(this IEnumerable<Player> players, List<GroupInfo> groups)
        {
            foreach (var player in players)
            {
                player.CalculateGroups(groups);
            }

            for (int i = 0; i < groups.Count; i++)
            {
                var number = i + 1;
                var groupPlayers = players.Where(x => x.ModeResults.ContainsKey(number));

                var maxScores = groupPlayers.Max(x => x.ModeResults[number].Scores);
                var maxScorePlayers = groupPlayers.Where(x => x.ModeResults[i + 1].Scores == maxScores);
                foreach (var p in maxScorePlayers)
                    p.ModeResults[i + 1].IsBestScores = true;

                var maxAvgspeed = groupPlayers.Max(x => x.ModeResults[number].AvgSpeed);
                var maxAvgspeedPlayers = groupPlayers.Where(x => x.ModeResults[number].AvgSpeed == maxAvgspeed);
                foreach (var p in maxScorePlayers)
                    p.ModeResults[i + 1].IsBestSpeed = true;
            }
        }

        public static void SetEnoughRaces(this IEnumerable<Player> players, int minimumRaces)
        {
            foreach (var player in players)
            {
                player.HasEnoughRaces = player.Results.Count >= minimumRaces;
            }
        }

        public static void SetSpeedAverageType(this IEnumerable<Player> players, AverageType type)
        {
            foreach (var player in players)
            {
                player.SpeedAverageType = type;
            }
        }

        public static void SetErRateAverageType(this IEnumerable<Player> players, AverageType type)
        {
            foreach (var player in players)
            {
                player.ErRateAverageType = type;
            }
        }

        public static void RecalculatePlayersPlaces(this IEnumerable<Player> players)
        {
            int racesCreated = players.First().RacesCreated;

            for (int r = 1; r <= racesCreated; r++)
            {
                var racePlayers = players.Where(x => x.Results.ContainsKey(r)).OrderBy(x => x.Results[r].RealPlace);

                int placeCounter = 1;
                foreach (var player in racePlayers)
                {
                    player.Results[r].CalculatedPlace = placeCounter++;
                }
            }
            players.Calculate();
        }
    }
}
