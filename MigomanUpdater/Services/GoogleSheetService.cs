using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using KlStatisticsLoader;

namespace MigomanUpdater.Services
{
    internal class GoogleSheetService : IGoogleSheetService
    {
        private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private readonly string ApplicationName = "MiGoMan Updater";
        
        public async Task UploadUsersResultsToSheetsAsync(List<string> modeIds, Dictionary<string, double> coeffs, List<UserMigomanResult> usersResults, DateTime startTimestamp)
        {
            UserCredential credential;
            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters.
            var spreadsheetId = "1AdsNcmQ96MnPQeMWz6rdRjF4V8bdY6VRrwokrT6krng";
            // var spreadsheetId = "12iKmWy5YhlYIciHFDkujLSLhRno-9NbG9mJiN6FKzyQ"; // for internal testing

            Console.WriteLine("Получение spreadsheet info");
            var spreadSheetRequest = service.Spreadsheets.Get(spreadsheetId);
            var spreadsheetInfo = await spreadSheetRequest.ExecuteAsync();
            
            Console.WriteLine("Обновление General Sheet");
            await WriteGeneralResultsAsync(modeIds, coeffs, usersResults, service, spreadsheetId, spreadsheetInfo);
            Console.WriteLine("Обновление Filtered Sheet");
            await WriteFilteredResultsAsync(modeIds, coeffs, usersResults, service, spreadsheetId, spreadsheetInfo);
            
            
            
            
            
            // all data uploaded, put timestamp
            Console.WriteLine("Обновление Completion Timestamps");
            await WriteCompletionTimeStampAsync(startTimestamp, service, spreadsheetId);
        }

        private static async Task WriteCompletionTimeStampAsync(DateTime startTimestamp, SheetsService service, string spreadsheetId)
        {
            SpreadsheetsResource.ValuesResource.UpdateRequest request;
            
            var valuesToWrite = new ValueRange();
            valuesToWrite.MajorDimension = "Rows";
            

            var endTimeStamp = DateTime.UtcNow;
            TimeZoneInfo moscowZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            startTimestamp = TimeZoneInfo.ConvertTimeFromUtc(startTimestamp, moscowZone);
            endTimeStamp = TimeZoneInfo.ConvertTimeFromUtc(endTimeStamp, moscowZone);

            valuesToWrite.Values = new List<IList<object>>()
            {
                new List<object>()
                {
                    $"{endTimeStamp.Date.ToShortDateString()}: {startTimestamp.ToLongTimeString()} - {endTimeStamp.ToLongTimeString()}"
                }
            };
            
            await WriteTimeStampToSheetAsync("General!B1:C1", service, spreadsheetId, valuesToWrite);
            await WriteTimeStampToSheetAsync("Filtered!B1:C1", service, spreadsheetId, valuesToWrite);
            foreach (var rank in Enum.GetValues(typeof(Rank)).Cast<Rank>().Where(r => r >= Rank.TaxiDriver))
            {
                await WriteTimeStampToSheetAsync($"{rank.ToString()}s!B1:C1", service, spreadsheetId, valuesToWrite);
            }
        }

        private static async Task WriteTimeStampToSheetAsync(string range, SheetsService service, string spreadsheetId, ValueRange valuesToWrite)
        {
            SpreadsheetsResource.ValuesResource.UpdateRequest request;

            valuesToWrite.Range = range;

            request =
                service.Spreadsheets.Values.Update(valuesToWrite, spreadsheetId, valuesToWrite.Range);
            request.ValueInputOption =
                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await request.ExecuteAsync();
        }


        private static async Task WriteGeneralResultsAsync(List<string> modeIds, Dictionary<string, double> coeffs, List<UserMigomanResult> usersResults, SheetsService service, string spreadsheetId, Spreadsheet spreadsheetInfo)
        {
            String range = "General!A4:V";

            var sheetId = spreadsheetInfo.Sheets.Where(s => s.Properties.Title == "General").Single().Properties.SheetId;

            var batchRequest = new BatchUpdateSpreadsheetRequest() {Requests = new List<Request>()};
            int position = 1;
            foreach (var userResults in usersResults.OrderByDescending(r => r.BestTournamentSpeedsTotal))
            {
                var rowRequest = new UpdateCellsRequest();
                rowRequest.Start = new GridCoordinate() {RowIndex = position + 2, ColumnIndex = 0, SheetId = sheetId};
                rowRequest.Fields = "*";

                var row = new RowData();
                rowRequest.Rows = new List<RowData>() {row};
                row.Values = new List<CellData>();
                var rowValues = row.Values;
                // A
                AddNumberValue(rowValues, position, userResults.Rank);
                // B
                AddFormulaValue(rowValues, userResults.UserHyperlink, userResults.Rank);
                // C
                AddNumberValue(rowValues, userResults.BestTournamentSpeedsTotal, userResults.Rank, true);
                // D
                AddNumberValue(rowValues, userResults.BestTournamentSpeedsTotal, userResults.Rank, true); // speed with coeff, hidden for now
                // E
                AddModeValue(GameType.AbraEn, modeIds, rowValues, userResults.GetBestTournamentSpeedWithStatHyperlink, userResults.Rank);
                // F
                AddModeValue(GameType.AbraEn, modeIds, rowValues, userResults.GetTournamentMileageWithStatHyperlink, userResults.Rank);
                // G
                AddModeValue(GameType.ShortEn, modeIds, rowValues, userResults.GetBestTournamentSpeedWithStatHyperlink, userResults.Rank);
                // H
                AddModeValue(GameType.ShortEn, modeIds, rowValues, userResults.GetTournamentMileageWithStatHyperlink, userResults.Rank);
                // I
                AddModeValue(GameType.HundredEn, modeIds, rowValues, userResults.GetBestTournamentSpeedWithStatHyperlink, userResults.Rank);
                // J
                AddModeValue(GameType.HundredEn, modeIds, rowValues, userResults.GetTournamentMileageWithStatHyperlink, userResults.Rank);
                // K
                AddModeValue(GameType.FrequencyEn, modeIds, rowValues, userResults.GetBestTournamentSpeedWithStatHyperlink, userResults.Rank);
                // L
                AddModeValue(GameType.FrequencyEn, modeIds, rowValues, userResults.GetTournamentMileageWithStatHyperlink, userResults.Rank);
                // M
                AddModeValue(GameType.NormalEn, modeIds, rowValues, userResults.GetBestTournamentSpeedWithStatHyperlink, userResults.Rank);
                // N
                AddModeValue(GameType.NormalEn, modeIds, rowValues, userResults.GetTournamentMileageWithStatHyperlink, userResults.Rank);
                // O
                AddModeValue(GameType.MiniMarathonEn, modeIds, rowValues, userResults.GetBestTournamentSpeedWithStatHyperlink, userResults.Rank);
                // P
                AddModeValue(GameType.MiniMarathonEn, modeIds, rowValues, userResults.GetTournamentMileageWithStatHyperlink, userResults.Rank);
                // Q
                AddNumberValue(rowValues, userResults.BestAllTimeMainSpeed, userResults.Rank);
                // R
                AddStringValue(rowValues, "TODO", userResults.Rank);
                // S
                AddStringValue(rowValues, "TODO", userResults.Rank);
                // T
                AddFormulaValue(rowValues, userResults.GetValueWithStatHyperlink(GameType.NormalRu, userResults.AllTimeMileages[GameType.NormalRu]), userResults.Rank, "Right");
                // U 
                AddFormulaValue(rowValues, userResults.GetValueWithStatHyperlink(GameType.NormalEn, userResults.AllTimeMileages[GameType.NormalEn]), userResults.Rank, "Right");
                // V
                AddNumberValue(rowValues, userResults.GetCombinedAllTimeMileage(coeffs), userResults.Rank);
                
                batchRequest.Requests.Add(new Request() {UpdateCells = rowRequest});
                
                position++;
            }
            
            var clearFormatRequest = new UpdateCellsRequest();
            clearFormatRequest.Range = new GridRange() {StartColumnIndex = 0, StartRowIndex = 3, SheetId = sheetId};
            clearFormatRequest.Fields = "userEnteredFormat";
            var clearFormatBatchRequest = new BatchUpdateSpreadsheetRequest() {Requests = new List<Request>()};
            clearFormatBatchRequest.Requests.Add(new Request{UpdateCells = clearFormatRequest});
            await service.Spreadsheets.BatchUpdate(clearFormatBatchRequest, spreadsheetId).ExecuteAsync();
            
            var clearRequestBody = new ClearValuesRequest();
            var clearRequest = service.Spreadsheets.Values.Clear(clearRequestBody, spreadsheetId, range);
            await clearRequest.ExecuteAsync();

            if (usersResults.Count == 0)
            {
                return;
            }
            
            var request =
                service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId);

            await request.ExecuteAsync();
        }

        private static void AddStringValue(IList<CellData> rowValues, string value, Rank rank)
        {
            AddCellValue(rowValues, new ExtendedValue() {StringValue = value}, rank, false, "Left");
        }
        private static void AddNumberValue(IList<CellData> rowValues, double value, Rank rank, bool bold = false)
        {
            AddCellValue(rowValues, new ExtendedValue() {NumberValue = value}, rank, bold, "Right");
        }

        private static void AddFormulaValue(IList<CellData> rowValues, string value, Rank rank, string horizontalAlignment = null)
        {
            AddCellValue(rowValues, new ExtendedValue() {FormulaValue = value}, rank, false, horizontalAlignment);
        }
        
        private static void AddCellValue(IList<CellData> rowValues, ExtendedValue value, Rank rank, bool bold, string horizontalAlignment = null)
        {
            rowValues.Add(new CellData() {UserEnteredValue = value, UserEnteredFormat = 
                new CellFormat()
                {
                    BackgroundColor = GetRankColor(rank),
                    TextFormat = new TextFormat()
                    {
                        Bold = bold, Underline = false, ForegroundColor = new Color(){ Red = 0, Green = 0, Blue = 0 }
                    },
                    HorizontalAlignment = horizontalAlignment
                }});
        }

        private static Color GetRankColor(Rank rank)
        {
            switch (rank)
            {
                case Rank.Novice:
                    return GetGoogleColor(0, 0, 0);
                case Rank.Amateur:
                    return GetGoogleColor(83, 155, 152);
                case Rank.TaxiDriver:
                    return GetGoogleColor(207, 255, 204);
                case Rank.Pro:
                    return GetGoogleColor(255, 253, 151);
                case Rank.Racer:
                    return GetGoogleColor(254, 202, 151);
                case Rank.Maniac:
                    return GetGoogleColor(253, 152, 203);
                case Rank.Superman:
                    return GetGoogleColor(202, 156, 255);
                case Rank.CyberRacer:
                    return GetGoogleColor(154, 206, 255);
                case Rank.ExtraCyber:
                    return GetGoogleColor(166, 166, 166);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), rank, null);
            }
        }

        private static Color GetGoogleColor(int red, int green, int blue)
        {
            const float divider = 255;
            return new Color() {Red = red / divider, Green = green / divider, Blue = blue / divider};
        }

        private static void AddModeValue(string gameType, List<string> modeIds, IList<CellData> rowValue, Func<string, string> getValueWithLink, Rank rank)
        {
            var value = GetModeValueWithLink(gameType, modeIds, getValueWithLink) ?? "Skipped";
            
            AddFormulaValue(rowValue, value, rank, "Right");
        }

        private static string GetModeValueWithLink(string gameType, List<string> modeIds, Func<string, string> getValueWithLink)
        {
            if (modeIds.Contains(gameType))
            {
                return getValueWithLink(gameType);
            }

            return null;
        }

        private static int? GetModeValue(string gameType, List<string> modeIds, Dictionary<string, int> values)
        {
            if (modeIds.Contains(gameType))
            {
                return values[gameType];
            }

            return null;
        }
        
        private static async Task WriteFilteredResultsAsync(List<string> modeIds, Dictionary<string, double> coeffs, List<UserMigomanResult> usersResults, SheetsService service,
            string spreadsheetId, Spreadsheet spreadsheetInfo)
        {
            String range = "Filtered!A3:N";

            var usersResultsFiltered = usersResults
                .Where(r => 
                (r.AllTimeMileages[GameType.NormalEn] >= 50 && r.GetCombinedAllTimeMileage(coeffs) >= 3000)
                && 
                (r.TournamentMileages
                    .All(m => m.Value >= (m.Key == GameType.MiniMarathonEn ? 2 : 3)))).ToList();

            var sheetId = spreadsheetInfo.Sheets.Where(s => s.Properties.Title == "Filtered").Single().Properties.SheetId;
            
            var batchRequest = new BatchUpdateSpreadsheetRequest() {Requests = new List<Request>()};
            int position = 1;
            
            foreach (var userResult in usersResultsFiltered.OrderByDescending(r => r.BestTournamentSpeedsTotal))
            {
                var rowRequest = new UpdateCellsRequest();
                rowRequest.Start = new GridCoordinate() {RowIndex = position + 1, ColumnIndex = 0, SheetId = sheetId};
                rowRequest.Fields = "*";
                
                var row = new RowData();
                rowRequest.Rows = new List<RowData>() {row};
                row.Values = new List<CellData>();
                var rowValues = row.Values;
                
                // A
                AddNumberValue(rowValues, position, userResult.Rank);
                // B
                AddFormulaValue(rowValues, userResult.UserHyperlink, userResult.Rank);
                // C
                AddNumberValue(rowValues, userResult.BestTournamentSpeedsTotal, userResult.Rank, true);
                // D
                AddNumberValue(rowValues, userResult.BestTournamentSpeedsTotal, userResult.Rank, true); // speed with coeff, hidden for now
                // E
                AddModeValue(GameType.AbraEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // F
                AddModeValue(GameType.ShortEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // G
                AddModeValue(GameType.HundredEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // H
                AddModeValue(GameType.FrequencyEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // I
                AddModeValue(GameType.NormalEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // J
                AddModeValue(GameType.MiniMarathonEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // K
                AddNumberValue(rowValues, userResult.BestAllTimeMainSpeed, userResult.Rank);
                // L
                AddStringValue(rowValues, "TODO", userResult.Rank);
                // M
                AddStringValue(rowValues, "TODO", userResult.Rank);
                // N
                AddModeValue(GameType.AbraEn, modeIds, rowValues, userResult.GetTournamentMileageWithStatHyperlink, userResult.Rank);

                batchRequest.Requests.Add(new Request() {UpdateCells = rowRequest});
                position++;
            }

            var clearFormatRequest = new UpdateCellsRequest();
            clearFormatRequest.Range = new GridRange() {StartColumnIndex = 0, StartRowIndex = 2, SheetId = sheetId};
            clearFormatRequest.Fields = "userEnteredFormat";
            var clearFormatBatchRequest = new BatchUpdateSpreadsheetRequest() {Requests = new List<Request>()};
            clearFormatBatchRequest.Requests.Add(new Request{UpdateCells = clearFormatRequest});
            await service.Spreadsheets.BatchUpdate(clearFormatBatchRequest, spreadsheetId).ExecuteAsync();
            
            var clearRequestBody = new ClearValuesRequest();
            var clearRequest = service.Spreadsheets.Values.Clear(clearRequestBody, spreadsheetId, range);
            await clearRequest.ExecuteAsync();

            if (usersResultsFiltered.Count > 0)
            {
                var request =
                    service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId);

                await request.ExecuteAsync();
            }

            var prosRacersManiacsMinPlayersCount = (int?)null;
            
            foreach (var rank in Enum.GetValues(typeof(Rank)).Cast<Rank>().Where(r => r >= Rank.TaxiDriver))
            {
                range = $"{rank.ToString()}s!A3:R";
                
                sheetId = spreadsheetInfo.Sheets.Where(s => s.Properties.Title == $"{rank.ToString()}s").Single().Properties.SheetId;
                
                Console.WriteLine($"Очистка {rank.ToString()}s Sheet");
                
                
                
                clearFormatRequest = new UpdateCellsRequest();
                clearFormatRequest.Range = new GridRange() {StartColumnIndex = 0, StartRowIndex = 2, SheetId = sheetId};
                clearFormatRequest.Fields = "userEnteredFormat";
                clearFormatBatchRequest = new BatchUpdateSpreadsheetRequest() {Requests = new List<Request>()};
                clearFormatBatchRequest.Requests.Add(new Request{UpdateCells = clearFormatRequest});
                await service.Spreadsheets.BatchUpdate(clearFormatBatchRequest, spreadsheetId).ExecuteAsync();
                
                clearRequestBody = new ClearValuesRequest();
                clearRequest = service.Spreadsheets.Values.Clear(clearRequestBody, spreadsheetId, range);
                await clearRequest.ExecuteAsync();

                var rankResults = usersResultsFiltered.Where(r => r.Rank == rank).ToList();
                if (rankResults.Count == 0)
                {
                    Console.WriteLine($"Нет результатов для {rank.ToString()}s Sheet");
                    continue;
                }

                if (rank == Rank.Pro || rank == Rank.Racer || rank == Rank.Maniac)
                {
                    if (!prosRacersManiacsMinPlayersCount.HasValue)
                    {
                        prosRacersManiacsMinPlayersCount = rankResults.Count;
                    }
                    else
                    {
                        prosRacersManiacsMinPlayersCount =
                            Math.Min(prosRacersManiacsMinPlayersCount.Value, rankResults.Count);
                    }
                } 
                
                Console.WriteLine($"Обновление {rank.ToString()}s Sheet");
                await WriteRankResultsAsync(modeIds, rankResults, service, spreadsheetId, sheetId.Value, rank >= Rank.Superman ? prosRacersManiacsMinPlayersCount : null);
            }
        }
        
        private static async Task WriteRankResultsAsync(List<string> modeIds, List<UserMigomanResult> usersResults, SheetsService service,
            string spreadsheetId, int sheetId, int? prosRacersManiacsMinPlayersCount = null)
        {

            var playersCount =
                (prosRacersManiacsMinPlayersCount.HasValue && prosRacersManiacsMinPlayersCount.Value > usersResults.Count)
                    ? prosRacersManiacsMinPlayersCount.Value
                    : usersResults.Count;
            var rankRewards = GetRankRewards(playersCount);

            // var fakeAbraEnMileages = new int[] {0, 31, 51, 101};
            var batchRequest = new BatchUpdateSpreadsheetRequest() {Requests = new List<Request>()};
            int position = 1;
            foreach (var userResult in usersResults.OrderByDescending(r => r.BestTournamentSpeedsTotal))
            {
                var rowRequest = new UpdateCellsRequest();
                rowRequest.Start = new GridCoordinate() {RowIndex = position + 1, ColumnIndex = 0, SheetId = sheetId};
                rowRequest.Fields = "*";
                
                var row = new RowData();
                rowRequest.Rows = new List<RowData>() {row};
                row.Values = new List<CellData>();
                var rowValues = row.Values;
                
                // var abraEnMileage = fakeAbraEnMileages[position % 4];
                var abraEnMileage = GetModeValue(GameType.AbraEn, modeIds, userResult.TournamentMileages) ?? 0;
                
                
                // A
                AddNumberValue(rowValues, position, userResult.Rank);
                // B
                AddFormulaValue(rowValues, userResult.UserHyperlink, userResult.Rank);
                // C
                AddNumberValue(rowValues, userResult.BestTournamentSpeedsTotal, userResult.Rank, true);
                // D
                AddNumberValue(rowValues, userResult.BestTournamentSpeedsTotal, userResult.Rank, true); // speed with coeff, hidden for now
                // E
                AddModeValue(GameType.AbraEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // F
                AddModeValue(GameType.ShortEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // G
                AddModeValue(GameType.HundredEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // H
                AddModeValue(GameType.FrequencyEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // I
                AddModeValue(GameType.NormalEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // J
                AddModeValue(GameType.MiniMarathonEn, modeIds, rowValues, userResult.GetBestTournamentSpeedWithStatHyperlink, userResult.Rank);
                // K
                AddNumberValue(rowValues, userResult.BestAllTimeMainSpeed, userResult.Rank);
                // L
                AddStringValue(rowValues, "TODO", userResult.Rank);
                // M
                AddStringValue(rowValues, "TODO", userResult.Rank);
                // N
                AddModeValue(GameType.AbraEn, modeIds, rowValues, userResult.GetTournamentMileageWithStatHyperlink, userResult.Rank);
                // O
                AddNumberValue(rowValues, abraEnMileage >= 50 ? (abraEnMileage >= 100 ? 10000 : 5000): 0, userResult.Rank);
                // P
                AddNumberValue(rowValues, position > 10 ? 0 : rankRewards[position], userResult.Rank);
                // Q
                AddNumberValue(rowValues, position > 10 && abraEnMileage >= 30 && abraEnMileage <= 50 ? 1000 : 0, userResult.Rank);
                // R
                AddFormulaValue(rowValues, $"=SUM(O{position + 2}:Q{position + 2})", userResult.Rank, "Right");

                batchRequest.Requests.Add(new Request() {UpdateCells = rowRequest});
                position++;
            }

            var request =
                service.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId);

            await request.ExecuteAsync();
        }

        private static Dictionary<int, int> GetRankRewards(int playersCount)
        {
            var rewardResults = new Dictionary<int, int>();

            var results = new int[] { 2000, 3000, 4000, 5000, 6000, 7000, 8000, 10000, 15000, 25000};

            var i = 0;
            for (var position = playersCount > 10 ? 10 : playersCount ; position > 0; position--)
            {
                rewardResults[position] = results[i];
                i++;
            }

            return rewardResults;
        }
    }
}