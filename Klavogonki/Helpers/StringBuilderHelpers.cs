using System.Text;

namespace Klavogonki
{
    public static class StringBuilderHelpers
    {
        public static void App(this StringBuilder sb, object o)
        {
            if (sb.Length != 0 && sb[sb.Length - 1] != '\n') sb.Append('\t');
            sb.Append(o);
        }
        public static void App(this StringBuilder sb, string o)
        {
            if (sb.Length != 0 && sb[sb.Length - 1] != '\n') sb.Append('\t');
            sb.Append(o);
        }
        public static void App(this StringBuilder sb, int o)
        {
            if (sb.Length != 0 && sb[sb.Length - 1] != '\n') sb.Append('\t');
            sb.Append(o);
        }
        public static void App(this StringBuilder sb, char o)
        {
            if (sb.Length != 0 && sb[sb.Length - 1] != '\n') sb.Append('\t');
            sb.Append(o);
        }

    }
}
