using System;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IHiddenStatService : IProgressNotifier
    {
        event EventHandler<EventArgs<HiddenStat>> PreliminaryHiddenStatUpdated;

        Task<HiddenStat> GetHiddenStat(int userId);
    }
}