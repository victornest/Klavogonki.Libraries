using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class UserSummaryService : IUserSummaryService
    {
        public async Task<FetchResult<UserSummary>> GetUserSummary(int id)
        {
            FetchResult<UserSummary> result; 
            try
            {
                string url = $"http://klavogonki.ru/api/profile/get-summary?id={id}";
                string json = await NetworkClient.DownloadstringAsync(url);
                if (json == "{\"err\":\"permission blocked\"}")
                    result = new FetchResult<UserSummary>(isOpen: false);

                else if (json == "{\"err\":\"invalid user id\"}")
                    result = new FetchResult<UserSummary>(userExists: false);

                else
                {
                    var us = JsonHelper.Deserialize<UserSummary>(json);
                    
                    if (us.Car.Car == 31) us.Car.Car = 1005; //замена костылей Arch'а
                    else if (us.Car.Car == 32) us.Car.Car = 1007;
                    else if (us.Car.Car == 1014) us.Car.Car = 1013;
                    us.Car.Name = CarsConstants.Cars[us.Car.Car];

                    Regex regex = new Regex("tuning\":(.*)},\"level");
                    string tunstr = regex.Match(json).Groups[1].Value;
                    //создание правильного us.car._tuning 
                    us.Car.Tuning = new int[10];
                    if (tunstr[0] == '[')
                    {
                        if (tunstr.Length == 3) us.Car.Tuning[0] = int.Parse(tunstr.Substring(1, 1));
                        else if (tunstr.Length > 3)
                        {
                            string[] temparr = tunstr.Substring(1, tunstr.Length - 2).Split(',');
                            int[] tempint = Array.ConvertAll<string, int>(temparr, int.Parse);
                            Array.Resize(ref tempint, 4);
                            us.Car.Tuning = tempint;
                        }
                    }
                    else
                    {
                        //Dictionary<string, int> dic = (Dictionary<string, int>)jss.Deserialize(tunstr, typeof(Dictionary<string, int>));
                        //foreach (KeyValuePair<string, int> entry in dic)
                        //{
                        //    us.Car.Tuning[int.Parse(entry.Key)] = entry.Value;
                        //}
                    }
                    //создание url хранения аэрографии если есть, если нет - то ""
                    us.Car.AeroUrl = Regex.Match(us.Car.Color, "'(.*)'").Groups[1].ToString();

                    result = new FetchResult<UserSummary>(us);
                }
            }
            catch (Exception)
            {
                result = new FetchResult<UserSummary>(isSuccessfulDownload: false);
            }
            return result;
        }
    }
}
