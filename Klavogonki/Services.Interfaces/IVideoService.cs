using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IVideoService : IProgressNotifier
    {
        Task<string> SortVideos(string text);
    }
}