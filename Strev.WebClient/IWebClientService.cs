using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Strev.WebClient
{
    public interface IWebClientService
    {
        CookieContainer CookieContainer { get; }

        string UserAgentPrefix { get; set; }

        string UserAgentPattern { get; set; }

        string GetUserAgent(string version);

        IWebClientRequest CreateRequest(string url);

        void AddSslPolicy(Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> sslPolicy);
    }
}