using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Klavogonki
{
    public static class NetworkClient
    {
        private static HttpClient client;

        private static HttpClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new HttpClient();
                }
                return client;
            }
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
