using Klavogonki;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Klavogonki.Hrustyashki
{
    public class HrustUpdater : IHrustUpdater
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly ISuccessService successService;
        private readonly IQuickStatService quickStatService;
        private readonly IOpenStatService openStatService;
        private readonly ITopService topService;

        private const string xlsPath = "Hrustyashki.xlsx";
        private const string txtPath = "Hrustyashki.txt";
        private const string addPlayersPath = "HrustyashkiAddPlayers.txt";
        private const string forumPatternPath = "HrustyashkiForumPattern.txt";
        private const string forumMessagePath = "HrustyashkiForumMessage.txt";

        public HrustUpdater(
            ISuccessService successService, 
            IQuickStatService quickStatService,
            IOpenStatService openStatService, 
            ITopService topService)
        {
            this.successService = successService;
            this.quickStatService = quickStatService;
            this.openStatService = openStatService;
            this.topService = topService;
        }

        public async Task<List<string>> UpdateHrustyashki()
        {
            Rank.SetMaxRank(Rank.Tachyon);
            List<string> filePaths = new List<string>();
            const int minTotalMileage = 3000;
           
            FileStream fs = new FileStream(xlsPath, FileMode.Open, FileAccess.Write);
            fs.Close();

            if (successService == null) return null;

            await successService.UpdateSuccesses();
            List<User> users = successService.Successes.Where(x => x.TotalMileage >= minTotalMileage).OrderByDescending(x => x.TotalMileage).Select(x => new User(x.Id, x.Nick)).ToList();

            var threeLastVocs = HrustConstants.ModeIds.Skip(HrustConstants.ModeIds.Length - 3).ToList();
            var threeLastVocsRacerRequirements = Enumerable.Range(22, 3).Select(x => HrustRequitements.GetRequirement(x, HrustRank.Racer)).ToList();

            ModeStatSettings modeStatInput = new ModeStatSettings() { ModeIds = threeLastVocs, NeedPeriodStat = true };
            var tops = await topService.GetBulkTop(modeStatInput, Period.Week);
            for (int i = 1; i < tops.Count; i++)
            {
                var topPlayers = tops[i].Where(x => x.TopResult >= threeLastVocsRacerRequirements[i]);
                foreach (var player in topPlayers)
                {
                    if (!users.Any(x => x.Id == player.UserId))
                        users.Add(new User(player.UserId, ""));
                }
            }

            if (File.Exists(addPlayersPath))
            {
                var lines = File.ReadAllLines(addPlayersPath, Encoding.UTF8);
                var players = lines.Select(x => int.TryParse(x, out int result) ? result : 0).Where(x => x > 0);

                foreach (var playerId in players)
                {
                    if (!users.Any(x => x.Id == playerId))
                        users.Add(new User(playerId, ""));
                }
            }

            List<HrustPlayer> results = await GetHrustResults(users);

            string hrTable = BuildTable(results);
            File.WriteAllText(txtPath, hrTable, Encoding.UTF8);

            // если что-то пошло не так, можно считать таблицу с txt файла
            //string HrTable = File.ReadAllText(txtPath, Encoding.UTF8); 
           
            hrTable = hrTable.Replace('\t', ';').Replace(" / ", " / ");

            Excel.Application excelapp = new Excel.Application();
            excelapp.DisplayAlerts = false;
            string xlsBigPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + xlsPath;
            Excel.Workbook excelworkbook = excelapp.Workbooks.Open(xlsBigPath, 0, false, 5,
                "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            excelapp.Calculation = Excel.XlCalculation.xlCalculationManual;
            Excel.Worksheet wsh = (Excel.Worksheet)excelworkbook.Worksheets.get_Item("Статистика всех игроков");
            wsh.Activate();

            Excel.Range range1 = wsh.Range[wsh.Cells[5, 3], wsh.Cells[5, 3]];
            range1.Select();
            ClipboardService.SetText(hrTable);

            wsh.Paste();

            Excel.Range range2 = wsh.Range[wsh.Cells[5, 3], wsh.Cells[5 + results.Count - 1, 3]];
            range2.TextToColumns(
                    range2,
                    Excel.XlTextParsingType.xlDelimited,
                    Excel.XlTextQualifier.xlTextQualifierNone,
                    false,
                    false,
                    true
                );
            excelworkbook.Save();

            // Копирование картинок из Excel
            int firstPlaceRow = 4;
            int imageHeight = 44; // lines
            int passed_24 = results.Count(x => x.TotalExercisesRank.Index >= 5);
            int passed_21_23 = results.Count(x => x.ExercisesStat.Count(y => y.Rank.Index >= 5) >= 21);
            int imageCounter = 0;

            for (int stRow = firstPlaceRow - 1, endRow = 0;
                endRow != firstPlaceRow + passed_21_23;
                stRow += imageHeight)
            {
                if (stRow < firstPlaceRow + passed_24)
                {
                    endRow = stRow + imageHeight - 1;
                    if (endRow > passed_24 + firstPlaceRow) endRow = firstPlaceRow + passed_24;
                }
                else
                {
                    stRow = firstPlaceRow + passed_24 + 1;
                    endRow = firstPlaceRow + passed_21_23;
                }

                Excel.Range range3 = wsh.Range[wsh.Cells[stRow, 4], wsh.Cells[endRow, 38]];
                range3.Copy();
                Image image = ClipboardService.GetImage();

                // Обрезка изображения
                Bitmap bmp = new Bitmap(image);
                Rectangle cropArea = new Rectangle(1, 1, bmp.Width - 1, bmp.Height - 1);
                image = bmp.Clone(cropArea, bmp.PixelFormat);

                image.Save(++imageCounter + ".png", ImageFormat.Png);
                filePaths.Add(imageCounter + ".png");
            }

            excelworkbook.Close();
            excelapp.Quit();

            Rank.SetMaxRank(Rank.ExtraCyber);
            return filePaths;
        }


        private async Task<List<HrustPlayer>> GetHrustResults(List<User> users)
        {
            List<string> closed = new List<string>();
            List<HrustPlayer> results = new List<HrustPlayer>();
            int counter = 0;

            ChangeProgress(new Progress(0, users.Count));
            foreach (var user in users)
            {
                var os = await openStatService.GetOpenStat(user.Id);
                HrustPlayer hs;
                if (os.IsOpen)
                {
                    string userNick = user.Nick;
                    if (string.IsNullOrEmpty(user.Nick))
                    {
                        var normalQs = await quickStatService.GetQuickStat(user.Id);
                        userNick = normalQs.Value.Nick;
                    }

                    hs = new HrustPlayer(user.Id, userNick, false);

                    hs.NormalStat.Mileage = os.Value.Gametypes["normal"].Info.NumRaces;
                    hs.NormalStat.Record = os.Value.Gametypes["normal"].Info.BestSpeed.Value;
                    hs.NormalStat.Rank = Rank.GetByRecord(os.Value.Gametypes["normal"].Info.BestSpeed.Value);

                    for (int i = 0; i < HrustConstants.ModeIds.Length; i++)
                    {
                        if (os.Value.Gametypes.ContainsKey(HrustConstants.ModeIds[i]))
                        {
                            hs.ExercisesStat[i].Record = os.Value.Gametypes[HrustConstants.ModeIds[i]].Info.BestSpeed.Value;
                            hs.ExercisesStat[i].Mileage = os.Value.Gametypes[HrustConstants.ModeIds[i]].Info.NumRaces;
                        }
                    }
                }
                else
                {
                    var normalQs = await quickStatService.GetQuickStat(user.Id);

                    hs = new HrustPlayer(user.Id, normalQs.Value.Nick, true);
                    hs.NormalStat.Record = normalQs.Value.Record;
                    hs.NormalStat.Mileage = normalQs.Value.Mileage;
                    hs.NormalStat.Rank = normalQs.Value.Rank;

                    for (int i = 23; i >= 0; i--)
                    {
                        var qs = await quickStatService.GetQuickStat(user.Id, HrustConstants.ModeIds[i]);
                        if (qs.Value.Mileage == 0)
                        {
                            closed.Add(user.Nick);
                            continue;
                        }
                        else
                        {
                            hs.ExercisesStat[i].Record = qs.Value.Record;
                            hs.ExercisesStat[i].Mileage = qs.Value.Mileage;
                        }
                    }
                }
                hs.Calculate();
                results.Add(hs);
                ChangeProgress(new Progress(++counter, users.Count));
            }
            results.Sort();
            return results;
        }

        private static string BuildTable(List<HrustPlayer> stats)
        {
            stats.Sort();
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            foreach (var stat in stats)
            {
                bool isCheated = stat.ExercisesStat.Any(x => x.IsCheated);
                sb.App(stat.NormalStat.Rank.Name);
                sb.App("");
                sb.App(++counter);
                sb.App(stat.Id);
                sb.App(stat.Nick + (isCheated ? "*" : ""));
                sb.App(stat.NormalStat.Record);
                sb.App(stat.NormalStat.Mileage);
                sb.App(stat.TotalExercisesRank.ShortName);
                sb.App((isCheated ? "  " : "") + stat.ExercisesRecordsSum + (isCheated ? "*" : ""));
                sb.App(stat.ExercisesTotalMileage);
                sb.App("");
                sb.App(stat.Progress + " / " + 24);
                for (int i = 0; i < 24; i++)
                {
                    sb.App(stat.ExercisesStat[i].Rank.Index > 0 ? stat.ExercisesStat[i].Rank.Index - 1 : 0);
                }
                for (int i = 0; i < 24; i++) sb.App(stat.ExercisesStat[i].Mileage);
                for (int i = 0; i < 24; i++) sb.App(stat.ExercisesStat[i].Record);
                for (int i = 0; i < 24; i++) sb.App(stat.ExercisesStat[i].IsCheated ? "АЗ" : "");
                if (stat.IsClosedStat) sb.App("закрытая");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string GetNewForumMessage(List<string> imageBBCodes)
        {
            string forumPattern = File.ReadAllText(forumPatternPath, Encoding.UTF8);
            string date = DateTime.Now.ToString("dd.MM.yyyy");
            string img1 = string.Join("\r\n", imageBBCodes.Take(imageBBCodes.Count - 1));
            string img2 = imageBBCodes.Last();

            string forumMessage = forumPattern.Replace("$img1$", img1).Replace("$img2$", img2).Replace("$date$", date);

            File.WriteAllText(forumMessagePath, forumMessage, Encoding.UTF8);

            return forumMessage;
        }
    }
}
