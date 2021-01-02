using System.Collections.Generic;

namespace Klavogonki
{
    public class ModeStat : List<Stat>
    {
        public ModeStat(Mode mode)
        {
            this.Mode = mode;
        }

        public Mode Mode { get; private set; }
    }

    public class ModeStatList : List<ModeStat>
    {
        public ModeStatList(StatType statType)
        {
            StatType = statType;
        }

        public StatType StatType { get; set; }
    }
}
