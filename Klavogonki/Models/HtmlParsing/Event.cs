using System;
using System.Text.RegularExpressions;

namespace Klavogonki
{
    public class Event
    {
        public string Url { get; private set; }
        public string ShortName { get; private set; }
        public DateTime DateTime { get; private set; }
        public string Markdown { get; private set; }

        public Topic Topic { get; set; } 

        public Event(string fullName, string url)
        {
            ShortName = (Regex.Replace(fullName, "\\[.+\\]", "")).Trim();

            string dateStr = Regex.Match(fullName, "\\d{1,2}\\.\\d{1,2}\\.\\d{2,4}").ToString();
            if (DateTime.TryParse(dateStr, out DateTime dt)) DateTime = dt;
            
            Url = url;
            Markdown = "[" + ShortName + "](" + url + ")";
        }

        public Event(string markdownString)
        {
            Markdown = markdownString;
            DateTime = DateTime.Today;
            Match match = Regex.Match(markdownString, "\\[(.+)\\]\\((.+)\\)");
            if (match.Success)
            {
                ShortName = match.Groups[1].ToString();
                Url = match.Groups[2].ToString();
            }
            else
            {
                ShortName = "";
                Url = "";
            }
        }
    }

}
