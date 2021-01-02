using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class PlayerService : IPlayerService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly IOpenStatService openStatService;

        public PlayerService(IOpenStatService openStatService)
        {
            this.openStatService = openStatService;
        }


        public async Task<int?> GetIdByNick(string nick)
        {
            string page = await NetworkClient.DownloadstringAsync("http://klavogonki.ru/.fetchuser?login=" + nick);

            var match = Regex.Match(page, "\\d+");
            int? result = null;
            if (match.Success) result = int.Parse(match.Value);
            return result;
        }

        public async Task<List<int?>> GetIdsByNicks(string[] nicks)
        {
            List<int?> userIds = new List<int?>();
            foreach (var nick in nicks)
            {
                int? userId = null;
                if (!string.IsNullOrEmpty(nick)) userId = await GetIdByNick(nick);
                userIds.Add(userId);
            }
            return userIds;
        }


        public async Task<List<PlayersComparison>> ComparePlayers(int userId1, int userId2)
        {
            var os1 = await openStatService.GetOpenStat(userId1);
            var os2 = await openStatService.GetOpenStat(userId2);

            var results = new List<PlayersComparison>();

            foreach (var mode in os1.Value.Gametypes)
            {
                var comparison = new PlayersComparison()
                {
                    Mode = new Mode(mode.Key, mode.Value.Name),
                    Stat1 = GetStatBase(mode.Value),
                    Stat2 = new ComparisonStat()
                };
                results.Add(comparison);
            }

            foreach (var mode in os2.Value.Gametypes)
            {
                var entry = results.Find(x => x.Mode.ModeId == mode.Key);
                if (entry != null)
                {
                    entry.Stat2 = GetStatBase(mode.Value);

                    if (entry.Stat1.Record.HasValue && entry.Stat2.Record.HasValue)
                    {
                        entry.Difference = new StatDifference(entry.Stat1.Record.Value - entry.Stat2.Record.Value, entry.Stat1.AvgSpeed.Value - entry.Stat2.AvgSpeed.Value);
                    }
                }
                else
                {
                    var ps = new PlayersComparison()
                    {
                        Mode = new Mode(mode.Key, mode.Value.Name),
                        Stat1 = new ComparisonStat(),
                        Stat2 = GetStatBase(mode.Value)
                    };
                    results.Add(ps);
                }
            }
            return results;

            ComparisonStat GetStatBase(OpenStat.Mode mode)
            {
                return new ComparisonStat
                {
                    Record = mode.Info.BestSpeed,
                    AvgSpeed = (int)mode.Info.AvgSpeed,
                    AvgErRate = mode.Info.AvgError,
                    Mileage = mode.NumRaces
                };
            }
        }


        //public string GetMedals(int userId)
        //{
        //    var qs = await quickStatService.GetQuickStat(userId, "normal");
        //    StringBuilder sb = new StringBuilder(256);
        //    foreach (Award aw in qs.Value.Awards)
        //    {
        //        // todo fix
        //        //sb.Append("[![");
        //        //sb.Append(aw.ModeId.Name);
        //        //sb.Append("](");
        //        //sb.Append(aw.ImageUrl);
        //        //sb.Append(")](");
        //        //sb.Append("http://klavogonki.ru/profile/");
        //        //sb.Append(userId);
        //        //sb.Append("/stats/?gametype=");
        //        //sb.Append(aw.ModeId.ModeId);
        //        //sb.Append(" \"");
        //        //sb.Append(aw.Name);
        //        //sb.Append(" «");
        //        //sb.Append(aw.ModeId.Name);
        //        //sb.Append("»\" ) ");
        //    }
        //    //[![Обычный](http://klavogonki.ru/wiki/images/a/aa/Award6.png)](http://klavogonki.ru/profile/231371/stats/?gametype=normal "Бриллиантовый шлем за 20000 текстов пробега в режиме «Обычный»" )

        //    return sb.ToString();
        //}

        public async Task<Image> GetAvatar(int id)
        {
            try
            {
                using (var client = new WebClient())
                {
                    byte[] avatar = await client.DownloadDataTaskAsync("http://img.klavogonki.ru/avatars/" + id + "_big.gif");
                    return Image.FromStream(new MemoryStream(avatar));
                }
            }
            catch (WebException)
            {
                //return Images.avatar_dummy;
                return null;
            }
        }


        public async Task<Image> GetCarImage(UserSummary us)
        {
            Bitmap backgr;
            //заливка фона цветом машин (в чате)
            backgr = new Bitmap(100, 50);
            Graphics gr = Graphics.FromImage(backgr);
            gr.Clear(ColorTranslator.FromHtml(us.Car.Color.Substring(0, 7)));

            //добавление аэро если оно есть
            if (!string.IsNullOrEmpty(us.Car.AeroUrl))
            {
                using (var client = new WebClient())
                {
                    byte[] byteaero = await client.DownloadDataTaskAsync("http://klavogonki.ru" + us.Car.AeroUrl);
                    Image aero = Image.FromStream(new MemoryStream(byteaero));
                    Bitmap baero = new Bitmap(aero);
                    backgr = MakeImage(new Size(100, 50), baero, backgr, 255);
                }
            }

            //загрузка картинки машины
            string carNum = us.Car.Car == 43 ? "43-3" : us.Car.Car.ToString();
            byte[] bytecar = new WebClient().DownloadData("http://klavogonki.ru/img/cars/" + carNum + ".png");
            Image icar = Image.FromStream(new MemoryStream(bytecar));
            Bitmap car_map = new Bitmap(icar);

            //верхняя строка в карте машины (кузов машины)
            Rectangle r = new Rectangle(us.Car.Tuning[0] * 100, 0, 100, 50);
            Bitmap car_row0 = car_map.Clone(r, car_map.PixelFormat);

            //смешиваем картинку машины с фоном
            Bitmap car = MakeImage(new Size(100, 50), car_row0, backgr, 255);

            for (int i = 1; i < us.Car.Tuning.Length; i++) //накладываем тюнингованые части
            {
                if (us.Car.Tuning[i] != 0)
                {
                    Rectangle rr = new Rectangle(us.Car.Tuning[i] * 100 - 100, i * 50, 100, 50);
                    Bitmap car_row_i = car_map.Clone(rr, car_map.PixelFormat);
                    car = MakeImage(new Size(100, 50), car_row_i, car, 255);
                }
            }
            return car;
        }
        private Bitmap MakeImage(Size ImgSize, Bitmap foreImg, Bitmap backImg, byte s)
        {
            // ImgSize = размер картинки-результата, обе исходные картинки приводятся к указанному размеру
            // s прозрачность накладываемого изображения foreImg от 0 (100%) до 255 (0%)
            // результат наследует Альфа-канал фонового изображения
            // наложение использует Альфа-канал накладываемого изображения
            Bitmap fimg = new Bitmap(foreImg, ImgSize);
            Bitmap bimg = new Bitmap(backImg, ImgSize);
            Bitmap bmp = new Bitmap(ImgSize.Width, ImgSize.Height);
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color fm = fimg.GetPixel(i, j);
                    Color bm = bimg.GetPixel(i, j);
                    byte af = (byte)(fm.A * s / byte.MaxValue);
                    byte a = bm.A;
                    byte r = (byte)((fm.R * af + bm.R * (byte.MaxValue - af)) / byte.MaxValue);
                    byte g = (byte)((fm.G * af + bm.G * (byte.MaxValue - af)) / byte.MaxValue);
                    byte b = (byte)((fm.B * af + bm.B * (byte.MaxValue - af)) / byte.MaxValue);
                    bmp.SetPixel(i, j, Color.FromArgb(a, r, g, b));
                }
            return bmp;
        }
    }
}
