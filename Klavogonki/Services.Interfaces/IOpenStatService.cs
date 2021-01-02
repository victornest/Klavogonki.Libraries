using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IOpenStatService
    {
        Task<FetchResult<OpenStat>> GetOpenStat(int userId);
    }
}