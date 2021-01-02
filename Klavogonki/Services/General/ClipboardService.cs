using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Klavogonki
{
    public static class ClipboardService
    {
        public static string GetText()
        {
            string text = null;
            DoSync(() =>
            {
                for (int i = 0; i < 500; i++)
                {
                    if (!string.IsNullOrEmpty(text)) break;
                    DoSync(() => text = Clipboard.GetText());
                    Thread.Sleep(10);
                }
            });
            return text;
        }

        public static void SetText(string text)
        {
            DoSync(() => Clipboard.SetText(text));
                //text, // Text to store in clipboard
                //false,       // Do not keep after our application exits
                //10,           // Retry 5 times
                //200));        // 200 ms delay between retries);
        }

        public static Image GetImage()
        {
            Image image = null;
            DoSync(() =>
            {
                for (int i = 0; i < 500; i++)
                {
                    if (image != null) break;
                    DoSync(() => image = Clipboard.GetImage());
                    Thread.Sleep(10);
                }
            });
            return image;
        }

        public static void Clear()
        {
            DoSync(Clipboard.Clear);
        }

        private static void DoSync(ThreadStart action)
        {
            Thread thread = new Thread(action);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
