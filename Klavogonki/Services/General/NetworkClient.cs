using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Klavogonki
{
    public static class NetworkClient
    {
        // private static CookieContainer cookieContainer = new CookieContainer();

        private static HttpClient client;

        private static HttpClient Client
        {
            get
            {
                if (client == null)
                {
                    var handler = new HttpClientHandler() {UseCookies = false};
                    client = new HttpClient(handler);
                }
                return client;
            }
        }

        public static async Task<string> PostFormAsync(string url, FormUrlEncodedContent formContent)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, url);
            message.Content = formContent;
            message.Headers.Add("Cookie", "user=vnest%3B36484aa0bf43a4a3d80734a5b382dbf2;");
            
            var response = await Client.SendAsync(message);
            
            // cookieContainer.Add(new Cookie("user", "vnest%3B36484aa0bf43a4a3d80734a5b382dbf2"));
            // var response = await Client.PostAsync(url, formContent);
            
            return await response.Content.ReadAsStringAsync();
        }


        public static async Task<string> DownloadstringAsync(string url)
        {
            var result = new StringResponse();
            var response = await Client.GetAsync(url);

            result.IsSuccessFul = response.IsSuccessStatusCode;
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string>DownloadstringAsyncOld(string url)
        {
            WebClient webClient = new WebClient()
            {
                Proxy = new WebProxy(),
                Encoding = Encoding.UTF8
            };
            return await webClient.DownloadStringTaskAsync(new Uri(url));
        }
    }
}
