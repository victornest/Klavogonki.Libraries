using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Klavogonki
{
    public class FriendsService : IFriendsService
    {
        public async Task<List<Friends.Friend>> GetFriends(int userId)
        {
            string json = await NetworkClient.DownloadstringAsync($"http://klavogonki.ru/api/profile/get-friends?userId={userId}");
            var friends = JsonHelper.Deserialize<Friends>(json);
            return friends.Users;
        }
    }
}
