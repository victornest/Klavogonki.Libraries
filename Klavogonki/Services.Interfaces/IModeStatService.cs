using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IModeStatService : IProgressNotifier
    {
        Task<ModeStatList> GetStatsByUsersAndModes(ModeStatSettings input);
    }
}