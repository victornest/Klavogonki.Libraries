using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IPlayerService : IProgressNotifier
    {
        Task<List<PlayersComparison>> ComparePlayers(int userId1, int userId2);
        Task<Image> GetAvatar(int id);
        Task<Image> GetCarImage(UserSummary us);
        Task<int?> GetIdByNick(string nick);
        Task<List<int?>> GetIdsByNicks(string[] nicks);
        //string GetMedals(int userId);
    }
}