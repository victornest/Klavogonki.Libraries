using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki
{
    public class UserInfoStatService : IUserInfoStatService
    {
        public event EventHandler<EventArgs<Progress>> ProgressChanged;

        protected void ChangeProgress(Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Progress>(progress));
        }

        private readonly IQuickStatService quickStatService;
        private readonly IFriendsService friendsService;
        private readonly IUserInfoService userInfoService;

        public UserInfoStatService(
            IQuickStatService quickStatService, 
            IFriendsService friendsService, 
            IUserInfoService userInfoService)
        {
            this.quickStatService = quickStatService;
            this.friendsService = friendsService;
            this.userInfoService = userInfoService;

            UserInfoStatDB = dataProvider.Read();
        }

        private static readonly IStorageProvider<List<UserInfoStat>> dataProvider = new StorageProvider<List<UserInfoStat>>("db_userInfoStat.dat");

        public static List<UserInfoStat> UserInfoStatDB { get; private set; }

        public async Task<List<UserInfoStat>> GetUserInfoStatList(List<int> userIds, bool needFriends)
        {
            ChangeProgress(new Progress(0, userIds.Count));

            var result = new List<UserInfoStat>(userIds.Count);
            for (int i = 0; i < userIds.Count; i++)
            {
                ChangeProgress(new Progress(i, userIds.Count));

                UserInfoStat uis = new UserInfoStat();

                uis.UserInfo = (await userInfoService.GetUserInfo(userIds[i])).Value;
                uis.QuickStat = (await quickStatService.GetQuickStat(userIds[i], "normal")).Value;
                uis.CalculateRanks();
                int sumRating = 0;
                if (needFriends && userIds[i] != 152219 && userIds[i] != 199825)
                {
                    var friendlist = await friendsService.GetFriends(userIds[i]);
                    for (int j = 0; j < friendlist.Count; j++)
                    {
                        ChangeProgress(new Progress(i, userIds.Count, j, friendlist.Count));

                        double pr0 = i * 100.0 / userIds.Count; //количество % в начале расчета игрока 33%
                        double pr1 = 100.0 / userIds.Count; //сколько один игрок занимает процентов
                        double pr2 = j * 100.0 / friendlist.Count; //сколько процентов друзей обработано
                        
                        int index = UserInfoStatDB.FindIndex(x => x.UserInfo.Id == friendlist[j].Id);
                        if (index >= 0)
                        {
                            sumRating += UserInfoStatDB[index].Ranks.Total;
                        }
                        else
                        {
                            UserInfoStat fuis = new UserInfoStat();
                            fuis.UserInfo = (await userInfoService.GetUserInfo(friendlist[j].Id)).Value;
                            fuis.QuickStat = (await quickStatService.GetQuickStat(friendlist[j].Id, "normal")).Value;
                            fuis.CalculateRanks();
                            UserInfoStatDB.Add(fuis);
                            sumRating += fuis.Ranks.Total;
                        }
                    }
                    uis.FriendsRating = friendlist.Count == 0 ? 0 : sumRating / friendlist.Count;
                }
                result.Add(uis);

                int ind = UserInfoStatDB.FindIndex(x => x.UserInfo.Id == uis.UserInfo.Id);
                if (ind >= 0) UserInfoStatDB[ind] = uis;
                else UserInfoStatDB.Add(uis);
            }
            ChangeProgress(new Progress(userIds.Count, userIds.Count));
            dataProvider.Save(UserInfoStatDB);
            return result;
        }
    }
}
