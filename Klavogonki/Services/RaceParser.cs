using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using System.Web.WebSockets;

namespace Klavogonki
{
    public class RaceParser : IRaceParser
    {
        public event EventHandler<EventArgs<string>> ShowMessage = delegate { };

        public int CountRaces(string firstRacePath)
        {
            string dpath = Path.GetDirectoryName(firstRacePath);
            string ext = Path.GetExtension(firstRacePath);

            int counter = 0;
            while (File.Exists($"{dpath}\\{++counter}{ext}")) {}
            return counter - 1;
        }

        public List<Player> ParseRaces(string firstRacePath, int racesCreated, out List<RaceInfo> raceModeInfos)
        {
            string dpath = Path.GetDirectoryName(firstRacePath);
            string ext = Path.GetExtension(firstRacePath);
            List<Player> players = new List<Player>();
            raceModeInfos = new List<RaceInfo>(racesCreated);

            for (int i = 1; i <= racesCreated; i++)
            {
                string htmlpath = dpath + "\\" + i + ext;
                string html = File.ReadAllText(htmlpath, Encoding.UTF8);
                var parseResults = ParseRace(html);

                raceModeInfos.Add(parseResults.RaceInfo);

                foreach (var result in parseResults.PlayerResults)
                {

                    if (result.RealPlace > 0)
                    {
                        var player = players.FirstOrDefault(x => x.Id == result.Id);
                        if (player == null)
                        {
                            player = new Player()
                            {
                                Id = result.Id,
                                Nick = result.Nick,
                                Rank = result.Rank,
                                RacesCreated = racesCreated
                            };
                            players.Add(player);
                        }

                        if (!player.Results.ContainsKey(i))
                        {
                            player.AddParsedResult(i, result);
                        }
                        else if (result.Nick != "Гость")
                        {
                            string message = $"Глюк клавогонок с двумя машинами с одним ником ({result.Nick}) в заезде № {i}. Вероятно, у какого-то другого игрока не будет результата в этом заезде, а у {result.Nick} может быть чужой результат";
                            ShowMessage(null, new EventArgs<string>(message));
                        }
                    }
                }
            }
            return players;
        }

        private RaceParsingResult ParseRace(string html)
        {
            var result = new RaceParsingResult
            {
                PlayerResults = new List<ResultParsed>(), 
                RaceInfo = new RaceInfo() 
            };

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode gameTypeSpan = doc.DocumentNode.SelectSingleNode("//td[@id='gamedesc']/span");
            string modeId = gameTypeSpan.GetAttributeValue("class", "0").Replace("gametype-", "");
            var SpanA = gameTypeSpan.SelectSingleNode("a");

            string modeName;
            if (SpanA == null) //станд режим
            {
                modeName = gameTypeSpan.InnerText;
            }
            else // словарь
            {
                modeId += "-" + Regex.Match(SpanA.GetAttributeValue("href", "0"), "\\d+");
                modeName = SpanA.InnerText;
            }
            result.RaceInfo.Mode = new Mode(modeId, modeName);

            result.RaceInfo.BookAuthor = doc.DocumentNode.SelectSingleNode("//div[@id='bookinfo']//div[@class='author']")?.InnerText;
            result.RaceInfo.BookName = doc.DocumentNode.SelectSingleNode("//div[@id='bookinfo']//div[@class='name']")?.InnerText;

            HtmlNode totalPlayersNode = doc.DocumentNode.SelectSingleNode("//div[@id='players-count-lbl']/span");
            
            if (!string.IsNullOrEmpty(totalPlayersNode?.InnerText))
            {
                var match = Regex.Match(totalPlayersNode.InnerText, @"\d+");
                if (match.Success) 
                    result.RaceInfo.TotalPlayers = int.Parse(match.Value);
            }

            HtmlNode pointsIncreaseNode = doc.DocumentNode.SelectSingleNode("//div[@id='players-count-lbl']/b");
            int pointsIncrease = 0;
            if (!string.IsNullOrEmpty(pointsIncreaseNode?.InnerText))
            {
                var match = Regex.Match(pointsIncreaseNode.InnerText, @"\d+");
                if (match.Success) pointsIncrease = int.Parse(match.Value);
            }

            HtmlNodeCollection players = doc.DocumentNode.SelectNodes("//div[@class='player other ng-scope']");
            if (players != null)
                players.Add(doc.DocumentNode.SelectSingleNode("//div[@class='player you ng-scope']"));// ng-scope
            else players = doc.DocumentNode.SelectNodes("//div[@class='player you ng-scope']");

            foreach (HtmlNode player in players)
            {
                ResultParsed resultParsed = new ResultParsed();
                resultParsed.Mode = new Mode(modeId, modeName);
                resultParsed.PointsIncrease = pointsIncrease;

                HtmlNode rating = player.SelectSingleNode("div[@class='rating']");
                HtmlNode car = player.SelectSingleNode("table[@class='car']");
                HtmlNode place = rating.SelectSingleNode("div/ins");
                HtmlNode nick = player.SelectSingleNode("table//a");
                if (nick != null)
                {
                    resultParsed.Nick = nick != null ? nick.InnerText : "Гость";
                    string id_str = nick.GetAttributeValue("href", "0");
                    resultParsed.Id = int.Parse(Regex.Match(id_str, "[0-9]+").ToString());
                    resultParsed.Rank = Rank.GetByIndex(int.Parse(nick.GetAttributeValue("class", "000000").Substring(4, 1)));
                }
                else
                {
                    continue;
                    //result.Nick = "Гость";
                    //result.Id = 0;
                    //result.Rank = Rank.GetByIndex(0);
                }

                if (place != null)
                {
                    string place_str = place.InnerText;
                    resultParsed.RealPlace = int.Parse(place_str.Substring(0, place_str.Length - 6));
                    resultParsed.Time = TimeSpan.Parse("00:" + rating.SelectSingleNode("div[@class='stats']/div").InnerText.Replace(" ", "").Replace("\r", "").Replace("\n", ""));
                    resultParsed.Speed = (int)Math.Round(double.Parse(rating.SelectSingleNode("div[@class='stats']/div[2]/span").InnerText, new NumberFormatInfo() { NumberDecimalSeparator = "," }));
                    resultParsed.ErCnt = int.Parse(rating.SelectSingleNode("div[@class='stats']/div[3]/span").InnerText);
                    resultParsed.ErRate = double.Parse(rating.SelectSingleNode("div[@class='stats']/div[3]/span[2]").InnerText, new NumberFormatInfo() { NumberDecimalSeparator = "," }) / 100;
                    result.RaceInfo.ArrivedPlayers++;
                }

                if (player.SelectSingleNode("div[@class='newrecord']//span[@class='']") != null)
                    resultParsed.IsRecord = true; //рекорд с записью или без

                int.TryParse(Regex.Match(car.GetAttributeValue("style", ""), "(?<=left: )\\d+(?=px)").ToString(), out int progress);
                resultParsed.Progress = (int)(progress / 4.8);
                //result.finished = progress >= 100; //style="top: 0px; left: 480px; "

                HtmlNode _imgcont = car.SelectSingleNode(".//div[@class='imgcont']");
                HtmlNode _left = car.SelectSingleNode(".//div[@class='imgcont leave']");
                resultParsed.HasLeftRace = _left != null;

                HtmlNode _noerror_fail = car.SelectSingleNode(".//img[@class='noerror-fail']");
                resultParsed.NoErrorFail = _noerror_fail != null;

                HtmlNode _i_style = car.SelectSingleNode(".//i");
                if (_i_style != null)
                {
                    int.TryParse(Regex.Match(_i_style.GetAttributeValue("title", ""), "\\d+").ToString(), out int _mileage);
                    resultParsed.Mileage = _mileage;
                }
                result.PlayerResults.Add(resultParsed);
            }
            return result;
        }

        public List<GroupInfo> GetGroups(List<RaceInfo> modeInfos)
        {
            var groups = new List<GroupInfo>();
            for (int i = 0; i < modeInfos.Count; i++)
            {
                if (i == 0 || modeInfos[i].Mode.ModeId != modeInfos[i - 1].Mode.ModeId)
                {
                    int length = 0;
                    int k = i;
                    while (k < modeInfos.Count && modeInfos[i].Mode.ModeId == modeInfos[k++].Mode.ModeId) length++;

                    var name = modeInfos[i].Mode.Name.Replace("Мини-марафон, 800 знаков", "Мини-марафон")
                                                     .Replace("Частотный словарь", "Частотный");
                    groups.Add(new GroupInfo()
                    {
                        Name = name,
                        StartPosition = i + 1,
                        Length = length
                    });
                }
            }

            return groups;
        }
    }
}
