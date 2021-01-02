using System.Collections.Generic;

namespace Klavogonki
{
    public interface IScoresCalculator
    {
        void CalculateScores(IEnumerable<Player> players);
    }
}
