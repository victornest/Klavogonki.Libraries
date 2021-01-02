using System.Collections.Generic;

namespace Klavogonki
{
    public class UserStat : List<Stat>
    { }

    public class UserStatList : List<UserStat>
    {
        public StatType StatType { get; set; }
    }
}
