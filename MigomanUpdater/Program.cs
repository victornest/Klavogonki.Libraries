using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KlStatisticsLoader;
using MigomanUpdater.Services;
using Ninject;

namespace MigomanUpdater
{
    internal class Program
    {
        private static StandardKernel kernel;

        private static Dictionary<string, double> mileageModesWithCoeffs_ = new Dictionary<string, double>();

        private static readonly List<string> modeIds_ = new List<string>
        {
            GameType.AbraEn,
            GameType.FrequencyEn,
            GameType.HundredEn,
            GameType.MiniMarathonEn,
            GameType.ShortEn,
            GameType.NormalEn 
        };

        private static List<string> allTimeMileageModeIds_;
        
        private static readonly DateTime fromDate = new DateTime(2021, 12, 6);
        private static readonly DateTime toDate = new DateTime(2021, 12, 12);
        private static readonly DateTime localFinishDate = new DateTime(2021, 12, 12, 22, 00, 00);
        
        public static void Main(string[] args)
        {
            kernel = new StandardKernel(new NinjectRegistrations());

            var fields = typeof(GameType).GetFields();

            allTimeMileageModeIds_ = fields.Select(p => p.GetValue(null).ToString()).ToList();
            
            foreach (string modeId in allTimeMileageModeIds_)
            {
                double coef;
                
                switch (modeId)
                {
                    case GameType.AbraRu:                    
                    case GameType.AbraEn:
                        coef = 1.1;
                        break;
                    case GameType.ShortRu:
                    case GameType.ShortEn:
                        coef = 0.4;
                        break;
                    case GameType.HundredRu:
                    case GameType.HundredEn:
                        coef = 0.2;
                        break;
                    case GameType.FrequencyRu:
                    case GameType.FrequencyEn:
                        coef = 0.5;
                        break;
                    case GameType.MiniMarathonRu:
                    case GameType.MiniMarathonEn:
                        coef = 3;
                        break;
                    case GameType.MarathonRu:
                        coef = 6;
                        break;
                    case GameType.LettersRu:
                        coef = 1.2;
                        break;
                    case GameType.ReferatsRu:
                        coef = 2;
                        break;
                    default:
                        coef = 1;
                        break;
                }

                mileageModesWithCoeffs_[modeId] = coef;
            }

            const int timeout = 30;

            var finish = false;  
            
            while (!finish)
            {
                UpdateMigomanResults().Wait();

                Task t = Task.Run(() =>
                {
                    Console.WriteLine($"Waiting {timeout} minutes");
                    Console.WriteLine($"Press '0' to abort the program");
                    Console.WriteLine($"Press any other key to force the update");
                    var key = Console.ReadLine();
                    if (key == "0")
                    {
                        finish = true;
                    }
                });

                TimeSpan ts = TimeSpan.FromMinutes(timeout);
                if (!t.Wait(ts))
                {
                    Console.WriteLine($"The {timeout} minutes timeout interval elapsed.");
                }

                var datetimeNow = DateTime.Now;
                if (datetimeNow >= localFinishDate)
                {
                    Console.WriteLine($"Tournament finished at {datetimeNow}");
                    break;
                }
            }
        }

        private static async Task UpdateMigomanResults()
        {
            var resultService = kernel.Get<IMigomanResultService>();

            Console.WriteLine("Получение результатов MiGoMan");
            var startTimestamp = DateTime.UtcNow;
            var results = await resultService.GetResultsAsync(modeIds_, allTimeMileageModeIds_, fromDate, toDate);
            
            var sheetService = kernel.Get<IGoogleSheetService>();
            Console.WriteLine("Обновление результатов в Google Sheets");
            await sheetService.UploadUsersResultsToSheetsAsync(modeIds_, mileageModesWithCoeffs_, results, startTimestamp);
        }
    }
}