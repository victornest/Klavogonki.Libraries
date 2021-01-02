using System;

namespace Klavogonki
{
    [Serializable]
    public class Voc : IComparable<Voc>
    {
        public int Position { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int MarksCnt { get; set; }
        public string Type { get; set; }
        public string AuthorNick { get; set; }
        public int AuthorId { get; set; }
        public int SnippetsCnt { get; set; }
        public int SymbolsCnt { get; set; }
        public string Description { get; set; }

        // только на странице словаря
        public int InUseCnt { get; set; }
        public string Text { get; set; }

        public int CompareTo(Voc voc)
        {
            return -this.InUseCnt.CompareTo(voc.InUseCnt);
        }
    }
}
