using Strev.WebClient.Core.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Strev.WebClient.Service
{
    public class WebClientService : IWebClientService
    {
        public string UserAgentPrefix
        {
            get
            {
                return _userAgentPrefix ?? "Strev.WebClient";
            }

            set
            {
                _userAgentPrefix = value;
            }
        }

        public string UserAgentPattern
        {
            get
            {
                return _userAgentPattern ?? string.Format("{0} - {1}", UserAgentPrefix, "{0}");
            }
            set
            {
                _userAgentPattern = value;
            }
        }

        public WebClientService()
        {
        }

        private bool _initialized = false;

        private readonly List<Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool>> SslPolicies = new List<Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool>>();

        private bool DefaultSslPolicy(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        private bool _callback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (SslPolicies.Count == 0)
            {
                return DefaultSslPolicy(sender, certificate, chain, sslPolicyErrors);
            }
            else
            {
                foreach (var sslPolicy in SslPolicies)
                {
                    if (!sslPolicy(sender, certificate, chain, sslPolicyErrors))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void AddSslPolicy(Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> sslPolicy) => SslPolicies.Add(sslPolicy);

        public void Init()
        {
            if (!_initialized)
            {
                ServicePointManager.ServerCertificateValidationCallback += _callback;
                _initialized = true;
            }
        }

        public void Dispose()
        {
        }

        public string GetUserAgent(string version) => string.Format(UserAgentPattern, version ?? "latest");

        private CookieContainer _cookieContainer = null;
        private string _userAgentPattern;
        private string _userAgentPrefix;

        public CookieContainer CookieContainer => _cookieContainer ?? (_cookieContainer = new CookieContainer());

        public Stream GetTimeoutStream(Stream internalStream, TimeSpan timeout)
        {
            return new TimeoutStream(internalStream, timeout);
        }

        private IAsyncPrimitive<T> GetAsyncPrimitive<T>(Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end)
        {
            return new AsyncPrimitive<T>(begin, end);
        }

        public T ExecSyncWithTimeout<T>(Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end, TimeSpan timeout)
        {
            return GetAsyncPrimitive(begin, end).ExecuteSync(timeout);
        }

        public void ExecSyncWithTimeout(Func<AsyncCallback, object, IAsyncResult> begin, Action<IAsyncResult> end, TimeSpan timeout)
        {
            GetAsyncPrimitive<object>(begin, arg => { end(arg); return null; }).ExecuteSync(timeout);
        }

        public HttpWebResponse GetResponseWithTimeout(HttpWebRequest webRequest, TimeSpan timeout) => (HttpWebResponse)ExecSyncWithTimeout
            (
                webRequest.BeginGetResponse,
                webRequest.EndGetResponse,
                timeout
            );

        public Stream GetRequestStreamWithTimeout(HttpWebRequest webRequest, TimeSpan timeout) => ExecSyncWithTimeout
            (
                webRequest.BeginGetRequestStream,
                webRequest.EndGetRequestStream,
                timeout
            );

        public IWebClientRequest CreateRequest(string url)
        {
            Init();
            return new WebClientRequest(this, url);
        }
    }
}