using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Klavogonki
{
    public class ForumService : IForumService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly IKgAutenticationService kgAuthorizationService;
        public ForumService(IKgAutenticationService kgAuthorizationService)
        {
            this.kgAuthorizationService = kgAuthorizationService;
        }

        public async Task<ForumPost> GetForumPost(string messageUrl)
        {
            ForumPost post = new ForumPost();
            post.MessageUrl = messageUrl;
            post.MessageId = Regex.Match(post.MessageUrl, "#post(\\d+)").Groups[1].ToString();
            post.PageHtml = await NetworkClient.DownloadstringAsync(messageUrl);

            post.MessageBBCode = Regex.Match(post.PageHtml, "<textarea id=text-" + post.MessageId + " style=\"display:none\">([\\s\\S]+?)</textarea>").Groups[1].ToString();

            post.AuthorName = Regex.Match(post.PageHtml, "id=username-" + post.MessageId + "\\s*>\\s*(.+?)\\s*</a>").Groups[1].ToString();
            post.TopicId = Regex.Match(post.PageHtml, "name=\"topic_id\" value=\"(\\d+)\">").Groups[1].ToString();
            return post;
        }

        public async Task<List<Event>> GetLastEventsByNick(string author, string nameSearchPattern = null)
        {
            return (await GetForumTopics("events", 3, nameSearchPattern)).Where(x => x.AuthorNick == author).Select(x => new Event(x.FullName, x.Link) { Topic = x }).OrderByDescending(x => x.DateTime).ToList();
        }

        public async Task<List<Topic>> GetForumTopics(string forumName, int maxPages = int.MaxValue, string nameSearchPattern = null)
        {
            List<Topic> result = new List<Topic>(128);
            HtmlDocument doc = new HtmlDocument();
            int pageCnt;
            int topicCnt = 0;
            int lastPage = 0;
            for (pageCnt = 1; pageCnt <= maxPages; pageCnt++)
            {
                string page = await NetworkClient.DownloadstringAsync($"http://klavogonki.ru/forum/{forumName}/page{pageCnt}");
                doc.LoadHtml(page);
                HtmlNodeCollection topics = doc.DocumentNode.SelectNodes("//tr[@class='item ']|//tr[@class='item even']");

                var lastPageNode = doc.DocumentNode.SelectSingleNode("//span[@class='page'][last()]");
                int.TryParse(lastPageNode.InnerText, out lastPage);
                ChangeProgress(new Progress(pageCnt, lastPage, topicCnt, 0));

                foreach (var topicNode in topics)
                {
                    Topic topic = ParseTopicNode(topicNode);

                    if (string.IsNullOrWhiteSpace(nameSearchPattern) ||
                        Regex.IsMatch(topic.FullName, nameSearchPattern, RegexOptions.IgnoreCase))
                    {
                        result.Add(topic);
                        topicCnt++;
                    }
                }

                if (topics.Count < 30) break;
            }
            ChangeProgress(new Progress(lastPage, lastPage, topicCnt, 0));
            return result;
        }

        private Topic ParseTopicNode(HtmlNode node)
        {
            Topic topic = new Topic();

            HtmlNode _title = node.SelectSingleNode("td[@class='title']");
            HtmlNode _name = _title.SelectSingleNode(".//a");
            HtmlNode _closed = _title.SelectSingleNode("img");
            topic.Closed = _closed != null;
            HtmlNode pinned = _title.SelectSingleNode("span[@class='topic-note']"); //null

            topic.Pinned = pinned != null;
            topic.StartDate = _title.SelectSingleNode("div").InnerText;
            topic.FullName = HttpUtility.HtmlDecode(_name.InnerText.Trim()); //"[16.01.16 21:03] Лига Суперменов №16"
            topic.ShName = (Regex.Replace(topic.FullName, "\\[.+\\]", "")).Trim();
            topic.Link = "http://klavogonki.ru" + _name.Attributes["href"].Value;
            topic.Id = int.Parse(Regex.Match(topic.Link, "\\d+").Value);

            HtmlNode author = node.SelectSingleNode("td[@class='author']/a");
            topic.AuthorNick = author.InnerText;
            string _authorId = author.Attributes["href"].Value;
            topic.AuthorId = int.Parse(Regex.Match(_authorId, "\\d+").Value);

            var postCntNode = node.SelectSingleNode("td[@class='post-cnt']");
            if (int.TryParse(postCntNode.InnerText, out int val))
                topic.PostCnt = val;
            HtmlNode _lastDate = node.SelectSingleNode("td[@class='last-post']/span");
            topic.LastPostDate = _lastDate == null ? "" : _lastDate.InnerText;

            topic.BBCodeLink = "[url=\"" + topic.Link + "\"]" + topic.FullName + "[/url]";
            return topic;
        }
         
        public void WriteTopicsToFile(List<Topic> topics, string forumName)
        {
            StringBuilder sb = new StringBuilder(1024);

            foreach (Topic t in topics)
            {
                sb.App(t.Id);
                sb.App(t.Link);
                sb.App(t.StartDate);
                sb.App(t.LastPostDate);
                sb.App(t.FullName);
                sb.App(t.AuthorId);
                sb.App(t.AuthorNick);
                sb.App(t.PostCnt);
                sb.App(t.Pinned ? "закреплена" : "");
                sb.App(t.Closed ? "закрыта" : "");
                sb.App(t.BBCodeLink);
                sb.AppendLine();
            }

            File.WriteAllText($"forum_{forumName}.txt", sb.ToString(), Encoding.UTF8);
        }

        public string EditForumPost(ForumPost post, string message)
        {
            string sdata = $"topic={post.TopicId}&subid={post.MessageId}&csrftoken=0cfedaa0&text={message}";
            string kgToken = kgAuthorizationService.Tokens[post.AuthorName];
            byte[] data = Encoding.UTF8.GetBytes(sdata);
            HttpWebRequest reqEdit = (HttpWebRequest)HttpWebRequest.Create("http://klavogonki.ru/ajax/forum-edit-post");
            reqEdit.Method = "POST";
            reqEdit.CookieContainer = new CookieContainer();
            reqEdit.CookieContainer.Add(new Cookie("user", kgToken, "/", "klavogonki.ru"));
            reqEdit.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            reqEdit.ContentLength = data.Length;
            using (Stream stream = reqEdit.GetRequestStream())
                stream.Write(data, 0, data.Length);
            HttpWebResponse respEdit = (HttpWebResponse)reqEdit.GetResponse();
            string sresp = "";
            using (StreamReader sr = new StreamReader(respEdit.GetResponseStream(), Encoding.UTF8))
                sresp = sr.ReadToEnd();
            return sresp;
        }
        public string PostNewMessage(string topicId, string topicUrl, string kgToken, string message)
        {
            //MultipartFormDataContent mfdc = new MultipartFormDataContent("----WebKitFormBoundaryNOpvTUIbWWaXPjw7");        
            //Logger.Log("Создание сообщения");
            //string sdata = "topic=" + topicId + "&subid=" + messageId + "&csrftoken=0cfedaa0&text=" + message;
            string sdata =
    @"------WebKitFormBoundaryNOpvTUIbWWaXPjw7
Content-Disposition: form-data; name=""topic_id""

" + topicId +
    @"------WebKitFormBoundaryNOpvTUIbWWaXPjw7
Content-Disposition: form-data; name=""act""

delete
------WebKitFormBoundaryNOpvTUIbWWaXPjw7
Content-Disposition: form-data; name=""move_topic_id""


------WebKitFormBoundaryNOpvTUIbWWaXPjw7
Content-Disposition: form-data; name=""text""

" + message + @"
------WebKitFormBoundaryNOpvTUIbWWaXPjw7
Content-Disposition: form-data; name=""send""

Отправить
------WebKitFormBoundaryNOpvTUIbWWaXPjw7--";


            byte[] data = UTF8Encoding.UTF8.GetBytes(sdata);
            HttpWebRequest reqEdit = (HttpWebRequest)HttpWebRequest.Create(topicUrl);
            reqEdit.Method = "POST";
            reqEdit.CookieContainer = new CookieContainer();
            reqEdit.CookieContainer.Add(new Cookie("user", kgToken, "/", "klavogonki.ru"));
            reqEdit.ContentType = "multipart/form-data; boundary=----WebKitFormBoundaryNOpvTUIbWWaXPjw7";
            reqEdit.ContentLength = data.Length;
            using (Stream stream = reqEdit.GetRequestStream())
                stream.Write(data, 0, data.Length);
            HttpWebResponse respEdit = (HttpWebResponse)reqEdit.GetResponse();
            string sresp = "";
            using (StreamReader sr = new StreamReader(respEdit.GetResponseStream(), Encoding.UTF8))
                sresp = sr.ReadToEnd();
            return sresp;
        }

    }
}
