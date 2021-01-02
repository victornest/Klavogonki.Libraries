using System;

namespace Klavogonki
{
    [Serializable]
    public class Award
    {
        public string ModeId { get; set; }
        public AwardType Type { get; set; }
    } 
}
