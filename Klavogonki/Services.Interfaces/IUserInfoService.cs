using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IUserInfoService
    {
        Task<FetchResult<UserInfo>> GetUserInfo(int id);
    }
}