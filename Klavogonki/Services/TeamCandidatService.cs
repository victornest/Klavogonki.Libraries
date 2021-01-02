using Klavogonki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface ITeamCandidatService : IProgressNotifier
    {
        Task<List<TeamCandidate>> GetTeamCandidats(List<int> excludingIds);
    }

    public class TeamCandidatService : ITeamCandidatService
    {
       private readonly IFriendsService friendsService;
       private readonly ITopService topService;
       private readonly IQuickStatService quickStatService;
       private readonly IPeriodStatService periodStatService;
       private readonly IForumService forumService;

        public TeamCandidatService(
            IFriendsService friendsService, 
            ITopService topService, 
            IQuickStatService quickStatService, 
            IPeriodStatService periodStatService,
            IForumService forumService)
        {
            this.friendsService = friendsService;
            this.topService = topService;
            this.quickStatService = quickStatService;
            this.periodStatService = periodStatService;
            this.forumService = forumService;
        }

        public event EventHandler<EventArgs<Progress>> ProgressChanged;
        protected void ChangeProgress(Progress progress) => ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));

        private readonly List<int> leadAccounts = new List<int>()
        {
            171789, // BigRace
            570549, // LittleBigRace
            332329, // -__Марафон__-
            298274, // Профсоюз
            548609, // Биатлон
            261337, // Гран-При
            572711, // МиГоМан
            234982, // CrashRace
            405687, // Slopestyle
            246730, // Лига_Маньяков
            594986, // АВТОтриал
        };

        List<TeamCandidate> Users = new List<TeamCandidate>();

       private int operationCount = 0;
       private int operationsCount = 4;

        private const int TopPlayersCount = 1500;

        public async Task<List<TeamCandidate>> GetTeamCandidats(List<int> excludingIds)
        {
            await GetLeadAccountsFriends();
            operationCount++;   
            await GetNormalWeakTop();
            operationCount++;

            await DefineTeamPlayers();

            await GetQuickStat();
            operationCount++;
            await GetPeriodStat();
            operationCount++;

            CountScores(excludingIds);

            return Users.Where(x => x.LeadFriendsCount >=1 && x.TopStat != null && x.HasEhoughMileage).OrderByDescending(x => x.Scores).ThenByDescending(x => x.Best5Results == null).ThenByDescending(x => x.LeadFriendsCount).ToList();
        }

        private async Task GetLeadAccountsFriends()
        {
            int counter = 0;
            foreach(var acc in leadAccounts)
            {
                var friends = await friendsService.GetFriends(acc);
                foreach(var friend in friends)
                {
                    var user = GetUser(friend.Id, friend.Login);
                    user.LeadFriendsCount++;
                }
                ChangeProgress(new Progress(operationCount, operationsCount, ++counter, leadAccounts.Count));
            }
        }

        private async Task GetNormalWeakTop()
        {
            var top = await topService.GetTop("normal", TopPlayersCount, Period.Week);

            foreach(var topResult in top)
            {
                var user = GetUser(topResult.Id, topResult.Nick);
                user.TopStat = topResult;
            }
        }

        private TeamCandidate GetUser(int id, string nick)
        {
            var user = Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                user = new TeamCandidate { Id = id, Nick = nick };
                Users.Add(user);
            }
            return user;
        }

        private async Task DefineTeamPlayers()
        {
            string mainTopicUrl = "http://klavogonki.ru/forum/events/4361/";
            List<int> teamPlayers = await GetTeamPlayers(mainTopicUrl);

            foreach (var user in Users)
            {
                if (teamPlayers.Contains(user.Id))
                    user.IsTeamPlayer = true;
            }

            int maxGamesNotToInvite = 3;
            var post = await forumService.GetForumPost($"{mainTopicUrl}#post3");
            var urlMatches = Regex.Matches(post.MessageBBCode, @"http://klavogonki.ru/forum/events/\d+");

            var lastGamesUrl = new List<string>();

            if (urlMatches.Count >= maxGamesNotToInvite)
            {
                for(int i = 1; i <= 3; i++)
                {
                    lastGamesUrl.Add(urlMatches[urlMatches.Count - i].Value);
                }
            }

            foreach(var url in lastGamesUrl)
            {
                var oldPlayers = await GetTeamPlayers(url);

                foreach (var user in Users)
                {
                    if (oldPlayers.Contains(user.Id))
                        user.IsOldTeamPlayer = true;
                }
            }
        }

        private async Task<List<int>> GetTeamPlayers(string topicUrl)
        {
            var html = await NetworkClient.DownloadstringAsync(topicUrl);

            var matches = Regex.Matches(html, "(?<=(.+)\n\\[list=1\\])[\\s\\S]+?(?=\\[/list\\])");

            List<int> result = new List<int>();

            for (int i = 0; i < matches.Count; i++)
            {
                var header = matches[i].Groups[1].Value;

                var captain = Regex.Match(header, "\\[size=3\\]([^,:\\[]+).+/(\\d+).+\\]([^]]+)\\[/color\\]");
                string capId = captain.Groups[2].Value;

                if (capId != "")
                {
                    result.Add(int.Parse(capId));
                }
                string[] lines = matches[i].Value.Trim().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {

                    Match m = Regex.Match(line, "(\\d+).+\\]([^]]+)\\[/color\\]");
                    string id = m.Groups[1].Value;
                    string nick = m.Groups[2].Value;
                    if (!string.IsNullOrEmpty(id))
                    {
                        result.Add(int.Parse(id));
                    }
                }
            }
            return result;
        }

        private async Task GetQuickStat()
        {
            int counter = 0;
            var users = Users.Where(x => !x.IsTeamPlayer && !x.IsOldTeamPlayer && x.LeadFriendsCount >= 1 && x.TopStat != null && x.TopStat.AvgErRate <= 0.035).ToList();
                
            foreach(var user in users)
            {
                var qs = await quickStatService.GetQuickStat(user.Id);
                if (qs.UserExists)
                {   
                    user.QuickStat = qs.Value;
                    user.HasTwoThirdEhoughMileage = user.QuickStat.Mileage >= GetMinimumMileage(user.QuickStat.Rank) * 0.666;
                    user.HasEhoughMileage = user.QuickStat.Mileage >= GetMinimumMileage(user.QuickStat.Rank);
                }
                ChangeProgress(new Progress(operationCount, operationsCount, ++counter, users.Count));
            }
        }

        private int GetMinimumMileage(Rank rank)
        {
            if (rank == Rank.Profi) return 750;
            else if (rank == Rank.Racer) return 1500;
            else if (rank >= Rank.Maniac) return 2000;
            else return int.MaxValue;
        }

        private async Task GetPeriodStat()
        {
            int counter = 0;
            var users = Users.Where(x => x.HasTwoThirdEhoughMileage).ToList();
            foreach (var user in users)
            {
                var stat = await periodStatService.GetDaysStat(user.Id, Mode.Normal);
                if (stat.IsOpen)
                {    
                    user.ThreeMonthsMileage = periodStatService.GetPeriodStat(stat.Value, from: DateTime.Now.AddMonths(-3), to: DateTime.Now).Mileage;
                    user.OneMonthMileage = periodStatService.GetPeriodStat(stat.Value, from: DateTime.Now.AddMonths(-1), to: DateTime.Now).Mileage;

                    var allTimeStat = periodStatService.GetPeriodStatLimited(stat.Value, maxMileage: 3000, maxSpeed: user.QuickStat.Record);
                    user.Best5Results = allTimeStat.BestResultsBelowMaxSpeed;

                    user.SaturdaysMileage = GetSaturdayMileage(stat.Value.Where(x => x.Date >= DateTime.Now.AddMonths(-3).Date).ToList());

                    if (user.ThreeMonthsMileage >= 400) user.HasEhoughMileage = true;
                }
                ChangeProgress(new Progress(operationCount, operationsCount, ++counter, users.Count));
            }
        }

        private int GetSaturdayMileage(List<DayStat> daysStat)
        {
            return daysStat.Where(x => x.Date.DayOfWeek == DayOfWeek.Saturday).Sum(x => x.Mileage);
        }

        void CountScores(List<int> excluding)
        {
            foreach (var user in Users)
            {
                user.Scores = GetDayTopScores(user) +
                              GetErrorsScores(user) +
                              GetMileageScores(user) +
                              GetThreeMonthsMileageScores(user) +
                              GetOneMonthMileageScores(user) +
                              GetSaturdaysScores(user) +
                              GetLeadFriendsScores(user);

                if (user.Scores < 0) user.Scores = 0;

                if (excluding.Contains(user.Id))
                    user.Scores *= -1;
                
                else if (user.ThreeMonthsMileage < 200 
                    || user.OneMonthMileage < 70 
                    || user.SaturdaysMileage < 30
                    || user.SaturdaysMileageRate < 1 / 7 * 0.2)
                    user.Scores = 0;                 
            }

            int GetDayTopScores(TeamCandidate user)
            {
                if (user.QuickStat?.DayTop.HasValue == true) return 1;
                else return 0;
            }
            int GetErrorsScores(TeamCandidate user)
            {
                if (user.TopStat?.AvgErRate <= 1.5) return 2;
                if (user.TopStat?.AvgErRate <= 2.5) return 1;
                else if (user.TopStat?.AvgErRate <= 3) return 0;
                else if (user.TopStat?.AvgErRate <= 3.25) return -2;
                else return -4;
            }
            int GetMileageScores(TeamCandidate user)
            {
                if (user.QuickStat != null)
                {
                    var rate = (double)user.QuickStat.Mileage / GetMinimumMileage(user.QuickStat.Rank);
                    if (rate >= 3) return 5;
                    else if (rate >= 2) return 3;
                    else if (rate >= 1.5) return 1;
                }
                 return 0;
            }
            int GetThreeMonthsMileageScores(TeamCandidate user)
            {
                if (user.ThreeMonthsMileage >= 3000) return 5;
                else if (user.ThreeMonthsMileage >= 2000) return 4;
                else if (user.ThreeMonthsMileage >= 1000) return 3;
                else if (user.ThreeMonthsMileage >= 500) return 2;
                else if (user.ThreeMonthsMileage >= 350) return 1;
                else if (user.ThreeMonthsMileage >= 300) return 0;
                else if (user.ThreeMonthsMileage >= 250) return -1;
                else return -2;
            }
            int GetOneMonthMileageScores(TeamCandidate user)
            {
                if (user.OneMonthMileage >= 1000) return 5;
                else if (user.OneMonthMileage >= 750) return 4;
                else if (user.OneMonthMileage >= 500) return 3;
                else if (user.OneMonthMileage >= 300) return 2;
                else if (user.OneMonthMileage >= 150) return 1;
                else if (user.OneMonthMileage >= 100) return 0;
                else if (user.OneMonthMileage >= 85) return -1;
                else return -2;
            }
            int GetSaturdaysScores(TeamCandidate user)
            {
                if (user.SaturdaysMileageRate >= 1 / 7 * 1.5) return 3;
                else if (user.SaturdaysMileageRate >= 1 / 7 * 1.25) return 2;
                else if (user.SaturdaysMileageRate >= 1 / 7 * 1.0) return 1;
                else if (user.SaturdaysMileageRate >= 1 / 7 * 0.8) return 0;
                else if (user.SaturdaysMileageRate >= 1 / 7 * 0.6) return -1;
                else if (user.SaturdaysMileageRate >= 1 / 7 * 0.4) return -2;
                else return -3;
            }
            int GetLeadFriendsScores(TeamCandidate user)
            {
                if (user.LeadFriendsCount >= 7) return 7;
                else if (user.LeadFriendsCount >= 5) return 6;
                else if (user.LeadFriendsCount == 4) return 5;
                else if (user.LeadFriendsCount == 3) return 4;
                else if (user.LeadFriendsCount == 2) return 2;
                else return 0;
            }

        }

    }
}
