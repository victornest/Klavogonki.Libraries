namespace Klavogonki.Hrustyashki
{
    internal static class HrustRequitements
    {
        public static int GetRequirement(int exerciseNumber, HrustRank hrustRank)
        {
            if ((int)hrustRank > 10) return int.MaxValue;
            return HrustConstants.Requirements[exerciseNumber - 1, (int)hrustRank];
        }

        public static HrustRank GetRank(int exerciseNumber, int record)
        {
            for (int i = 10; i >= 5; i--)
                if (record >= HrustConstants.Requirements[exerciseNumber - 1, i])
                    return (HrustRank)i;
            return HrustRank.Novice;
        }
    }
}
