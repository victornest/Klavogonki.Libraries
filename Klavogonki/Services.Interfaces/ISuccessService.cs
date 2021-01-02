using System.Threading.Tasks;

namespace Klavogonki
{
    public interface ISuccessService : IProgressNotifier
    {
        Successes Successes { get; }
        Task UpdateSuccesses();
    }
}