using System;
using System.Globalization;
using System.Linq;

namespace Klavogonki
{
    [Serializable]
    public class Rank : IComparable<Rank>, IComparable
    {
        static Rank()
        {
            list = new Rank[]
            {
                Guest, Novice, Amateur, TaxiDriver, Profi, Racer, Maniac, Superman, CyberRacer, ExtraCyber, Tachyon
            };
            SetMaxRank(ExtraCyber);
        }

        private static readonly Rank[] list;

        public static Rank[] List { get; private set; }

        public static Rank Guest { get; } = new Rank(0, "Гость", "Гость", "000000", 15);
        public static Rank Novice { get; } = new Rank(1, "Новичок", "Новичок", "8D8D8D", 15);
        public static Rank Amateur { get; } = new Rank(2, "Любитель", "Любитель", "4F9A97", 34);
        public static Rank TaxiDriver { get; } = new Rank(3, "Таксист", "Таксист", "187818", 35);
        public static Rank Profi { get; } = new Rank(4, "Профи", "Профи", "8C8100", 36);
        public static Rank Racer { get; } = new Rank(5, "Гонщик", "Гонщик", "BA5800", 40);
        public static Rank Maniac { get; } = new Rank(6, "Маньяк", "Маньяк", "BC0143", 38);
        public static Rank Superman { get; } = new Rank(7, "Супермен", "Супер", "5E0B9E", 39);
        public static Rank CyberRacer { get; } = new Rank(8, "Кибергонщик", "Кибер", "2E32CE", 37);
        public static Rank ExtraCyber { get; } = new Rank(9, "Экстракибер", "Экстра", "061956", 47);
        public static Rank Tachyon { get; } = new Rank(10, "Тахион", "Тахион", "061956", 47);


        public int Index { get; private set; }
        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public string HexColor { get; private set; }
        public byte BackColorIndex { get; private set; }

        private Rank(int index, string name, string shortName, string hexColor, byte backColorIndex)
        {
            Index = index;
            Name = name;
            ShortName = shortName;
            HexColor = hexColor;
            BackColorIndex = backColorIndex;
        }

        public static Rank GetByName(string name)
        {
            name.Replace("Клавомеханик", "Кибергонщик");
            var rank = List.FirstOrDefault(x => x.Name == name);
            return rank ?? List.First();
        }

        public static Rank GetByHexColor(string hexColor)
        {
            if (int.TryParse(hexColor, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out _))
            {
                var rank = List.FirstOrDefault(x => x.HexColor == hexColor);
                return rank;
            }
            return List.First();
        }

        public static Rank GetByIndex(int index)
        {
            if (index < 0) index = 0;
            else if (index >= List.Length) index = List.Length - 1;
            return List[index];
        }

        public static Rank GetByRecord(int record)
        {
            int index = 0;
            if (record > 0)
            {
                index = record / 100 + 1;
                if (index >= List.Length)
                    index = List.Length - 1;
            }
            return List[index];
        }

        public static Rank GetByBackColorIndex(byte backColorIndex)
        {
            var rank = List.FirstOrDefault(x => x.BackColorIndex == backColorIndex);
            return rank ?? List.First();
        }

        public static void SetMaxRank(Rank maxRank)
        {
            if (maxRank == ExtraCyber)
            {
                List = list.Take(list.Length - 1).ToArray();
            }
            else if (maxRank == Tachyon)
            {
                List = list;
            }
            else
            {
                throw new NotSupportedException();
            }
        }


        public int CompareTo(Rank rank)
        {
            return Index.CompareTo(rank.Index);
        }

        public int CompareTo(object rank)
        {
            return Index.CompareTo(((Rank)rank).Index);
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Rank rank && rank.Index == Index;
        }

        public override int GetHashCode() => Index;

        public static bool operator >(Rank a, Rank b) => a?.Index > b?.Index;
        public static bool operator <(Rank a, Rank b) => a?.Index < b?.Index;
        public static bool operator >=(Rank a, Rank b) => a?.Index >= b?.Index;
        public static bool operator <=(Rank a, Rank b) => a?.Index <= b?.Index;
        public static bool operator ==(Rank a, Rank b) => a?.Index == b?.Index;
        public static bool operator !=(Rank a, Rank b) => a.Index != b.Index;
    }
}
