using System;
using System.Linq;

namespace Klavogonki
{
    public static class ConverterHelpers
    {
        public static string CommaJoin(this int[] array)
        {
            return array?.Any() ?? false ? string.Join(",", array) : "";
        }

        public static int[] CommaSplit(this string str)
        {
            if (str != null)
            {
                var ids = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                return ids.Where(x => int.TryParse(x, out _)).Select(int.Parse).ToArray();
            }
            return new int[0];
        }
    }
}
