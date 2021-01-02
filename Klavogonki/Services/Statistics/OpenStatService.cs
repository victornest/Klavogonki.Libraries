using System;
using System.Net;
using System.Threading.Tasks;

namespace Klavogonki
{
    //класс для получения открытой статистики по всем режимам
    public class OpenStatService : IOpenStatService
    {
        public async Task<FetchResult<OpenStat>> GetOpenStat(int userId)
        {
            FetchResult<OpenStat> result;
            string address = $"http://klavogonki.ru/api/profile/get-stats-overview?userId={userId}";
            try
            {
                string json = await NetworkClient.DownloadstringAsync(address);
                if (json == "{\"err\":\"permission blocked\"}")
                    result = new FetchResult<OpenStat>(isOpen: false);

                else if (json == "{\"err\":\"invalid user id\"}")
                    result = new FetchResult<OpenStat>(userExists: false);

                else
                {
                    json = json.Replace("\"\":", "\"unknown_mode\":");

                    var os = JsonHelper.Deserialize<OpenStat>(json); ////62156  260725
                    //http://klavogonki.ru/api/profile/get-stats-overview?userId=62156

                    //http://klavogonki.ru/api/profile/get-stats-overview?userId=215941 игрок с null в avg_speed, best_speed, avg_error
                    result = new FetchResult<OpenStat>(os);
                }
            }
            catch (Exception)
            {
                result = new FetchResult<OpenStat>(isSuccessfulDownload: false);
            }
            return result;
        }
    }

}
