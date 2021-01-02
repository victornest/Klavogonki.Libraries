using System.Collections.Generic;

namespace Klavogonki
{    
    public abstract class OneResultBasedScoresCalculator : IScoresCalculator
    {
        public void CalculateScores(IEnumerable<Player> players)
        {
            foreach (var player in players)
            {
                foreach (var result in player.Results)
                {
                    result.Value.Scores = this.GetResultScores(result.Value);
                }
            }
            players.Calculate();
        }

        protected abstract int GetResultScores(Result result);
    }

    public class BiathlonScoresCalculator : OneResultBasedScoresCalculator
    {
        protected override int GetResultScores(Result result)
        {
            int scores = 0;
            if (result.CalculatedPlace.HasValue && result.CalculatedPlace <= EventConstants.BiathlonScores.Count)
                scores = EventConstants.BiathlonScores[result.CalculatedPlace.Value];
            return scores;
        }
    }

    public class CrashRaceScoresCalculator : OneResultBasedScoresCalculator
    {
        private readonly CrashRaceSettings settings;

        public CrashRaceScoresCalculator(CrashRaceSettings settings)
        {
            this.settings = settings;
        }

        protected override int GetResultScores(Result result)
        {
            int scores = 0;            
            if (result.Time.TotalSeconds <= 90)
            {
                if (result.RealPlace >= 1 && result.RealPlace <= 5) scores = settings.GroupsScores[0];
                else if (result.RealPlace >= 6 && result.RealPlace <= 10) scores = settings.GroupsScores[1];
                else if (result.RealPlace >= 11) scores = settings.GroupsScores[2];
                if (result.ErCnt == 0) scores += settings.NoErrorScores;
            }
            return scores;
        }
    }

    public class Formula1ScoresCalculator : OneResultBasedScoresCalculator
    {
        protected override int GetResultScores(Result result)
        {
            int scores = 0;
            if (result.Speed >= 400)
            {
                if (result.RealPlace <= EventConstants.Formula1Scores.Count )
                    scores = EventConstants.Formula1Scores[result.RealPlace];
                else if (result.Mode.ModeId == "noerror") scores = 1;
            }
            return scores;
        }
    }

    public class KokScoresCalculator : OneResultBasedScoresCalculator
    {
        protected override int GetResultScores(Result result)
        {
            int scores = 0;
            if (result.ErCnt == 0)
                scores = 1;
            return scores;
        }
    }

    public class LigaScoresCalculator : OneResultBasedScoresCalculator
    {
        protected override int GetResultScores(Result result)
        {
            int scores = 0;
            if (result.RealPlace <= EventConstants.BiathlonScores.Count
                && result.Speed >= 350)
                scores = EventConstants.BiathlonScores[result.RealPlace];
            return scores;
        }
    }

    public class XpressScoresCalculator : OneResultBasedScoresCalculator
    {
        protected override int GetResultScores(Result result)
        {
            int scores = 0;
            if ( result.RealPlace <= EventConstants.Formula1Scores.Count
                && (result.Mode.ModeId == "voc-5539" || result.Mode.ModeId == "voc-14878"))
                scores = EventConstants.Formula1Scores[result.RealPlace];
            return scores;
        }
    }    
}
