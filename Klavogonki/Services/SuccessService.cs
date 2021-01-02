using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class SuccessService : ISuccessService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));

        }
        private const string defaultPath = "db_successfulPlayers.dat";

        private readonly IStorageProvider<Successes> dataProvider = new StorageProvider<Successes>(defaultPath);

        public Successes Successes { get; private set; }

        public SuccessService()
        {
            Successes = dataProvider.Read();
        }

        public async Task UpdateSuccesses()
        {
            var oldData = Successes;
            var newData = await GetSuccesses();
            if (oldData.Count > 0) SetComparisonData(oldData, newData);
            Successes = newData;
            dataProvider.Save(Successes);
        }


        private async Task<Successes> GetSuccesses()
        {
            var result = new Successes() { DateTime = DateTime.Now };

            int progressTotalPages = 0;
            ChangeProgress(new Progress(0, 0, result.Count));

            for (int p = 1; ; p++)
            {
                ChangeProgress(new Progress(p - 1, progressTotalPages, result.Count));

                string html = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/top/success/468/" + p.ToString());

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var _table = doc.DocumentNode.SelectSingleNode("//div[@id='toplist'] | //dl[@id='toplist']");
                HtmlNodeCollection players = _table.SelectNodes(".//tr[@class='other']");
                if (players == null) break;

                if (progressTotalPages == 0)
                {
                    var lastPageNode = doc.DocumentNode.SelectSingleNode("//div[@class='pages']/span[last()]");
                    if (lastPageNode != null)
                        progressTotalPages = int.Parse(lastPageNode.InnerText);
                }

                foreach (var plNode in players) //30 штук
                {
                    var posNode = plNode.SelectSingleNode("td[@class='pos']");
                    var nameNode = plNode.SelectSingleNode("td[@class='name']/a");

                    var progressNode = plNode.SelectSingleNode("td[@class='highlight']/strong"); // словарь
                    var avgSpeedNode = plNode.SelectSingleNode("td[5]");
                    var mileageNode = plNode.SelectSingleNode("td[6]");

                    Success success = new Success
                    {
                        Place = int.Parse(posNode.InnerText.Replace(".", "")),
                        Id = int.Parse(Regex.Match(nameNode.Attributes["onmouseover"].Value, "\\d+").Value),
                        Nick = nameNode.InnerText.Trim(),
                        Progress = int.Parse(progressNode.InnerText),
                        AvgSpeed = int.Parse(Regex.Match(avgSpeedNode.InnerText, "\\d+").Value),
                        TotalMileage = int.Parse(Regex.Match(mileageNode.InnerText, "\\d+").Value)
                    };

                    result.Add(success);
                }
                if (!html.Contains("\">следующая</a>")) break;
            }

            ChangeProgress(new Progress(progressTotalPages, progressTotalPages, result.Count));
            return result;
        }

        private void SetComparisonData(Successes old, Successes current)
        {
            foreach (var player in current)
            {
                var last = old.Find(x => x.Id == player.Id);
                player.IsNew = last == null;
                if (last != null) player.MileageIncrease = player.TotalMileage - last.TotalMileage;
            }
        }

        // todo убрать отсюда
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
