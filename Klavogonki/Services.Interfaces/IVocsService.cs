using System.Collections.Generic;
using System.Threading.Tasks;

namespace Klavogonki
{
    public interface IVocsService : IProgressNotifier
    {
        Voc GetVocById(int vocId);
        Voc GetVocByName(string vocName);
        List<Voc> GetVocsByPattern(string vocNamePattern);
        Task<string> GetVocsLanguage(string[] vocIds);
        Task<string> GetVocsPopularity(string text);
        Task<string> GetVocsPopularityAndSort(string[] lines);
        Task UpdateList();
    }
}