using System;
using System.Collections.Generic;
using System.Linq;

namespace Klavogonki
{
    [Serializable]
    public class Mode
    {
        public string Name { get; private set; }
        public string ModeId { get; private set; }
        public int? VocId { get; private set; }
        public bool IsVoc { get; private set; }

        public static Mode[] StandardModes { get; } = new Mode[]
        {
            new Mode(Normal, "Обычный"),
            new Mode(Noerror, "Безошибочный"),
            new Mode(Sprint, "Спринт"),
            new Mode(Marathon, "Марафон"),
            new Mode(Abra, "Абракадабра"),
            new Mode(Chars, "Буквы"),
            new Mode(Referats, "Яндекс.Рефераты"),
            new Mode(Digits, "Цифры")
        };

        public const string Normal = "normal";
        public const string Noerror = "noerror";
        public const string Sprint = "sprint";
        public const string Marathon = "marathon";
        public const string Abra = "abra";
        public const string Chars = "chars";
        public const string Referats = "referats";
        public const string Digits = "digits";


        public Mode(string modeId, string name)
        {
            this.Name = name;

            modeId = modeId.Replace("voc-", "");
            if (int.TryParse(modeId, out int vocId))
            {
                this.ModeId = "voc-" + modeId;
                this.IsVoc = true;
                this.VocId = vocId;
            }
            else
            {
                this.ModeId = modeId;
                this.IsVoc = false;
                this.VocId = null;
            }
        }
        public Mode(string modeId)
            : this(modeId, "")
        { }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
