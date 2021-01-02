using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IUserSummaryService
    {
        Task<FetchResult<UserSummary>> GetUserSummary(int id);
    }
}