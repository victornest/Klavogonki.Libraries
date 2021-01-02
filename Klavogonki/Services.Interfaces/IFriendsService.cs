using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IFriendsService
    {
        Task<List<Friends.Friend>> GetFriends(int userId);
    }
}