using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class VideoService : IVideoService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly IQuickStatService quickStatService;

        public VideoService(IQuickStatService quickStatService)
        {
            this.quickStatService = quickStatService;
        }

        public async Task<string> SortVideos(string text)
        {
            string input = text.Replace("\r", "").Replace("\n[hide]", "[hide]").Replace("[/hide][img]", "[/hide]\n[img]").Replace("[/hide] [img]", "[/hide]\n[img]");
            string[] lines = input.Split('\n');
            List<Video> videoList = new List<Video>();
            int counter = 0;
            foreach (string line in lines)
            {
                ChangeProgress(new Progress(counter++, lines.Length));

                string id = Regex.Match(line, "klavogonki.ru/u/#/(\\d+)/\"").Groups[1].ToString();
                if (id == "") continue;

                var aj = await quickStatService.GetQuickStat(int.Parse(id), "normal");
                int record = aj.Value?.Record ?? 0;

                var video = new Video() { Text = line, Record = record };
                if (aj.Value?.Id == 215625 || aj.Value?.Id == 326740) video.Record = 800;
                else if (aj.Value?.Id == 319684 && record == 0) video.Record = 600;
                videoList.Add(video);
            }
            videoList.Sort();
            StringBuilder sb = new StringBuilder(500);
            foreach (Video v in videoList)
            {
                sb.Append(v.Text);
                sb.Append("\r\n");
            }
            ChangeProgress(new Progress(lines.Length, lines.Length));
            return sb.ToString().Replace("[/hide]\r\n[img]", "[/hide][img]");
        }

        public static string MakeVideoPreview(string text)
        {
            var matches = Regex.Matches(text, "watch\\?v=([a-zA-Z0-9_-]+)\"\\]\\[b\\]([^\\]]+)\\[");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < matches.Count; i++)
            {
                var videoId = matches[i].Groups[1];
                sb.Append("[url=\"https://www.youtube.com/watch?v=" + videoId + "\"][img]http://img.youtube.com/vi/" + videoId + "/default.jpg[/img][/url]");
                if (i % 7 == 6) sb.Append("\r\n\r\n");
                else sb.Append("\t");
            }
            return sb.ToString();
        }

    }
}
