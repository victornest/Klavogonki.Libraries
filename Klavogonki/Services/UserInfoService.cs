using System;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class UserInfoService : IUserInfoService
    {
        public async Task<FetchResult<UserInfo>> GetUserInfo(int id)
        {
            FetchResult<UserInfo> result;
            string address = "http://klavogonki.ru/api/profile/get-index-data?userId=" + id;
            try
            {
                string json = await NetworkClient.DownloadstringAsync(address);
                if (json == "{\"err\":\"hidden profile\"}")
                    result = new FetchResult<UserInfo>(isOpen: false);

                else if (json == "{\"err\":\"invalid user id\"}")
                    result = new FetchResult<UserInfo>(userExists: false);

                else
                {
                    var ui = JsonHelper.Deserialize<UserInfo>(json);
                    ui.Id = id;
                    result = new FetchResult<UserInfo>(ui);
                }
            }
            catch (Exception)
            {
                result = new FetchResult<UserInfo>(isSuccessfulDownload: false);
            }
            return result;
        }
    }
}
