using System;
using System.Collections.Generic;

namespace Klavogonki
{
    public interface IRaceParser
    {
        event EventHandler<EventArgs<string>> ShowMessage;
        int CountRaces(string firstRacePath);
        List<Player> ParseRaces(string firstRacePath, int racesCount, out List<RaceInfo> raceModeInfos);
        List<GroupInfo> GetGroups(List<RaceInfo> modeInfos);
    }
}