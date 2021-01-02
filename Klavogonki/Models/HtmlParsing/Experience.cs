using System;

namespace Klavogonki
{
    [Serializable]
    public class Experience
    {
        public string Date { get; set; }
        public int Position { get; set; }
        public int Id { get; set; }
        public string Nick { get; set; }
        public int ExperienceSum { get; set; }
        public int Competitions { get; set; }
        public int Bonuses { get; set; }
        public double AvgExperience { get; set; }
    }
}
