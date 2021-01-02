using System.Collections.Generic;

namespace Klavogonki
{
    public static class EventConstants 
    {
        public const string RallyCross = "Ралли-Кросс";
        public const string Xpress = "English_Xpress";
        public const string Crash = "CrashRace";
        public const string F1 = "Formula-1";
        public const string F2 = "Формула-2";
        public const string Prof = "Профсоюз";
        public const string LigaM = "Лига_Маньяков";
        public const string LigaS = "Лига_Суперменов";
        public const string MiG = "Мир_Гонщиков";
        public const string EH = "ЭХЪ";
        public const string Intersteno = "Интерстено-клуб";
        public const string GrandPrix = "Гран-При";
        public const string Maraths = "-__Марафон__-";
        public const string Slopestyle = "Slopestyle";
        public const string Kok = "К_о_К";
        public const string KomCenter = "КомЦентр";

        public static readonly List<string> AccsSayingRes = new List<string>
        {
            RallyCross, Xpress, Crash, F1, F2, Prof, LigaM, LigaS, MiG, EH, Intersteno, GrandPrix, Maraths, Slopestyle, Kok
        };

        public static readonly List<string> AccsLimitingRaces = new List<string>
        {
            EH, Intersteno, Maraths, Xpress
        };

        public const string SpeedSheetName = "Зачет по скорости";
        public const string ErrorRateSheetName = "Зачет по точности";
        public const string ErrorCountSheetName = "Ошибки";
        public const string RealPlaceSheetName = "Места реальные";
        public const string CalculatedPlaceSheetName = "Места пересчитанные";
        public const string SymbolsSheetName = "Длина текстов";
        public const string TimeSheetName = "Время заездов";

        public const string SpeedColumnName = "Средняя скорость";
        public const string ErrorRateColumnName = "Средний % ошибок";
        public const string ErrorCountColumnName = "Количество ошибок";
        public const string RealPlaceColumnName = "Среднее место";
        public const string CalculatedPlaceColumnName = "Среднее место";
        public const string SymbolsColumnName = "Суммарная длина";
        public const string TimeColumnName = "Среднее время";

        public static readonly int[] CrashScores = new int[] { 9, 8, 7, 1 };

        public static readonly Dictionary<int, int> Formula1Scores = new Dictionary<int, int>()
        {
            { 1, 25},
            { 2, 18},
            { 3, 15},
            { 4, 12},
            { 5, 10},
            { 6, 8},
            { 7, 6},
            { 8, 4},
            { 9, 2},
            { 10, 1}
        };

        public static readonly Dictionary<int, int> BiathlonScores = new Dictionary<int, int>()
        {
            { 1, 60},
            { 2, 54},
            { 3, 48},
            { 4, 43},
            { 5, 40},
            { 6, 38},
            { 7, 36},
            { 8, 34},
            { 9, 32},
            { 10, 31},
            { 11, 30},
            { 12, 29},
            { 13, 28},
            { 14, 27},
            { 15, 26},
            { 16, 25},
            { 17, 24},
            { 18, 23},
            { 19, 22},
            { 20, 21},
            { 21, 20},
            { 22, 19},
            { 23, 18},
            { 24, 17},
            { 25, 16},
            { 26, 15},
            { 27, 14},
            { 28, 13},
            { 29, 12},
            { 30, 11},
            { 31, 10},
            { 32, 9},
            { 33, 8},
            { 34, 7},
            { 35, 6},
            { 36, 5},
            { 37, 4},
            { 38, 3},
            { 39, 2},
            { 40, 1}
        };
    }
}
