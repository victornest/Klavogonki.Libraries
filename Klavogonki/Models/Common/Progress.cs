namespace Klavogonki
{
    public class Progress
    {
        public int Count { get; private set; }
        public int Total { get; private set; }
        public int Percent { get; private set; }
        public bool Completed => Count == Total && Total > 0;

        public Progress SecondaryProgress { get; private set; }

        public Progress(int percent)
        {
            this.Percent = percent;
        }

        public Progress(double percent)
        {
            this.Percent = (int)percent;
        }

        public Progress(int count, int total)
        {
            this.Count = count;
            this.Total = total;
            int percent = total == 0 ? 0 : count * 100 / total;
            this.Percent = percent <= 100 ? percent : 100;
        }

        public Progress(int count, int total, int count2)
            : this(count, total, count2, 0)
        {
        }

        public Progress(int count, int total, int count2, int total2)
            : this(count, total)
        {
            this.SecondaryProgress = new Progress(count2, total2);
        }
    }

}
