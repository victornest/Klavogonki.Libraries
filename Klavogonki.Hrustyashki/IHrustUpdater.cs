using Klavogonki;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki.Hrustyashki
{
    public interface IHrustUpdater : IProgressNotifier
    {
        Task<List<string>> UpdateHrustyashki();
    }
}