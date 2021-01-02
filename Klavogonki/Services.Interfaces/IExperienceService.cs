using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IExperienceService : IProgressNotifier
    {
        Task UpdateExperienceData(int maxPages);
    }
}