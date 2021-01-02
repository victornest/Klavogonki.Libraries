using System;

namespace Klavogonki
{
    public class Video : IComparable<Video>
    {
        public string Text { get; set; }
        public int Record { get; set; } 
        public int CompareTo(Video video)
        {
            return this.Record.CompareTo(video.Record) * -1;
        }
    }
}
