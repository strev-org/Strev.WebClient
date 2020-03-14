using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Strev.WebClient
{
    public interface IWebClientRequest : IDisposable
    {
        IWebClientRequest Get();

        IWebClientRequest Post();

        IWebClientRequest Put();

        IWebClientRequest Delete();

        IWebClientRequest Option();

        IWebClientRequest SetMethod(string method);

        IWebClientRequest SetContentType(string contentType);

        IWebClientRequest SetUserAgent(string userAgent);

        IWebClientRequest SetUserAgentVersion(string userAgentVersion);

        IWebClientRequest SetPostData(string postData);

        IWebClientRequest SetPostData(byte[] value);

        IWebClientRequest SetPostFilename(string filename);

        IWebClientRequestPostData StartPostData();

        IWebClientRequest SetGetData(string key, string value);

        IWebClientRequest SetGetData(IEnumerable<KeyValuePair<string, string>> dict);

        IWebClientRequest AddHeader(HttpRequestHeader httpRequestHeader, string value);

        IWebClientRequest AddHeader(string header, string value);

        IWebClientRequest AddHeader(string value);

        IWebClientRequest UseCookieContainer(CookieContainer cookieContainer);

        IWebClientRequest UseClientCertificate(X509Certificate2 clientCertificate);

        IWebClientRequest Accept(HttpStatusCode statusCode);

        IWebClientRequest Reject(HttpStatusCode statusCode);

        IWebClientRequest Accept(params HttpStatusCode[] statusCodes);

        IWebClientRequest Reject(params HttpStatusCode[] statusCodes);

        HttpStatusCode GetStatusCode();

        string GetAsString();

        byte[] GetAsByteArray();

        void GetAsFile(string filename, FileMode fileMode, FileAccess fileAccess);
    }
}