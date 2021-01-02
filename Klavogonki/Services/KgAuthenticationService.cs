using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Klavogonki
{
    public class KgAuthenticationService : IKgAutenticationService
    {
        public Dictionary<string, string> Tokens { get; private set; } = new Dictionary<string, string>();

        public bool CheckAuthByToken(string login, string token, string url)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "GET";
            req.ContentType = "text/html; charset=utf-8";
            req.CookieContainer = new CookieContainer();
            if (token != null) req.CookieContainer.Add(new Cookie("user", token, "/", "klavogonki.ru"));
            string html;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            using (StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                html = sr.ReadToEnd();

            if (Regex.IsMatch(html, "<input name=\"name\" value=\"" + login + "\" disabled>"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Authenticate(string login, string password)
        {
            string url = "http://klavogonki.ru/login";
            string formData = "login=" + login + "&pass=" + password + "&submit_login=Войти";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ProtocolVersion = new Version(1, 0);
            req.Method = "POST";
            req.AllowAutoRedirect = false;
            req.CookieContainer = new CookieContainer(); //обязательно для приема Cookies!
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] data = Encoding.UTF8.GetBytes(Uri.EscapeUriString(formData));
            req.ContentLength = data.Length;
            using (var stream = req.GetRequestStream())
                stream.Write(data, 0, data.Length);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            req = null;

            Cookie cookUser = resp.Cookies["user"];
            if (cookUser != null)
            {
                if (cookUser.Domain[0] == '.') cookUser.Domain = cookUser.Domain.Substring(1);
                string token = cookUser.Value;

                if (Tokens.ContainsKey(login)) 
                    Tokens[login] = token;
                else 
                    Tokens.Add(login, token);

                return true;
            }
            return false;
        }

    }
}
