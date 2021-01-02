using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IUserInfoStatService : IProgressNotifier
    {
        Task<List<UserInfoStat>> GetUserInfoStatList(List<int> userIds, bool needFriends);
    }
}