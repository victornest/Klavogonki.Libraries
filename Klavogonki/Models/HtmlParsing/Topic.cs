using System;

namespace Klavogonki
{
    public class Topic : IComparable<Topic>
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ShName { get; set; }
        public string Link { get; set; }
        public int AuthorId { get; set; }
        public string AuthorNick { get; set; }
        public int PostCnt { get; set; }
        public string StartDate { get; set; }
        public string LastPostDate { get; set; }
        public bool Pinned { get; set; }
        public bool Closed { get; set; }
        public string BBCodeLink { get; set; }

        public int CompareTo(Topic topic)
        {
            return this.Id.CompareTo((topic.Id));
        }
    }
}
