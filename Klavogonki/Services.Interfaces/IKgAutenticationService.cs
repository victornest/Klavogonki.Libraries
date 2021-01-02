using System.Collections.Generic;

namespace Klavogonki
{
    public interface IKgAutenticationService
    {
        Dictionary<string, string> Tokens { get; }

        bool CheckAuthByToken(string login, string token, string url);
        bool Authenticate(string login, string password);
    }
}