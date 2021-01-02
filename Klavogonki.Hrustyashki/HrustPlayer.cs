using System;
using System.Linq;

namespace Klavogonki.Hrustyashki
{
    public class HrustPlayer : IComparable<HrustPlayer>
    {
        public HrustPlayer(int id, string nick, bool isClosed)
        {
            Id = id;
            Nick = nick;
            IsClosedStat = isClosed;

            NormalStat = new HrustStat();

            ExercisesStat = new HrustStat[24];

            for (int i = 0; i < 24; i++)
            {
                ExercisesStat[i] = new HrustStat() { Rank = Rank.GetByIndex(1) };
            }
        }

        public int Id { get; private set; }
        public string Nick { get; private set; }

        public HrustStat NormalStat { get; private set; }

        public HrustStat[] ExercisesStat { get; private set; }

        public Rank TotalExercisesRank { get; private set; }
        
        // Количество пройденных упражнений на следующий ранг
        public int Progress { get; private set; } 

        public int ExercisesTotalMileage { get; private set; } 

        public int ExercisesRecordsSum { get; private set; } 

        public bool IsClosedStat { get; private set; }


        public void Calculate()
        {
            for (int i = 0; i < ExercisesStat.Length; i++)
            {
                var stat = ExercisesStat[i];
                var rank = HrustRequitements.GetRank(i + 1, stat.Record);
                stat.Rank =  Rank.GetByIndex((int)rank);
                var requirement = HrustRequitements.GetRequirement(i + 1, rank);
                stat.IsCheated = requirement > 0 && stat.Record > requirement * 1.2 + 100;
            }

            ExercisesTotalMileage = ExercisesStat.Sum(x => x.Mileage);

            ExercisesRecordsSum = ExercisesStat.Where(x => !x.IsCheated).Sum(x => x.Record);

            TotalExercisesRank = Rank.GetByIndex(ExercisesStat.Min(x => x.Rank.Index));

            var nextRankNumber = TotalExercisesRank.Index + 1;
            if (nextRankNumber < 5) nextRankNumber = 5;

            Progress = ExercisesStat.Count(x => x.Rank.Index >= nextRankNumber);
        }

        public int CompareTo(HrustPlayer player)
        {
            int result = -TotalExercisesRank.Index.CompareTo(player.TotalExercisesRank.Index);
            if (result == 0) result = -Progress.CompareTo(player.Progress);
            if (result == 0) result = -ExercisesRecordsSum.CompareTo(player.ExercisesRecordsSum);
            return result;
        }
    }
}
