using Newtonsoft.Json.Linq;
using Strev.WebClient.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Strev.WebClient.Service
{
    public class WebClientRequest : IWebClientRequest
    {
        private WebClientService WebClientService { get; set; }

        private HttpWebRequest Request { get; set; }

        private TimeSpan Timeout { get; set; } = new TimeSpan(0, 5, 0);

        private string Method { get; set; }

        private CookieContainer CookieContainer { get; set; }

        private string ContentType { get; set; }

        private string UserAgent { get; set; }

        private string UserAgentVersion { get; set; }

        private bool KeepAlive { get; set; } = true;

        private byte[] PostData { get; set; }

        private List<X509Certificate2> ClientCertificates { get; set; } = new List<X509Certificate2>();

        private bool HasSupportGZip { get; set; } = true;

        private List<KeyValuePair<HttpRequestHeader, string>> HeadersByHttpRequestHeader { get; set; } = new List<KeyValuePair<HttpRequestHeader, string>>();
        private List<KeyValuePair<string, string>> HeadersByString { get; set; } = new List<KeyValuePair<string, string>>();
        private List<string> Headers { get; set; } = new List<string>();

        private string Url { get; set; }

        private List<KeyValuePair<string, string>> GetParameters { get; set; } = new List<KeyValuePair<string, string>>();

        private HashSet<HttpStatusCode> AcceptedStatusCode { get; set; } = null;
        private HashSet<HttpStatusCode> RejectedStatusCode { get; set; } = new HashSet<HttpStatusCode>();

        private string GetCompleteUrl()
        {
            var urlBuilder = new StringBuilder(Url);
            var hasParameters = Url.Contains("?");

            foreach (var getParameter in GetParameters)
            {
                urlBuilder.Append(hasParameters ? "&" : "?");
                hasParameters = true;
                urlBuilder.Append(string.Format("{0}={1}", getParameter.Key, HttpUtility.UrlEncode(getParameter.Value)));
            }

            return urlBuilder.ToString();
        }

        public WebClientRequest(WebClientService webClientService, string url)
        {
            WebClientService = webClientService;
            Url = url;
        }

        private void EnsureParametersSetBeforeCall()
        {
            Request = (HttpWebRequest)WebRequest.Create(GetCompleteUrl());
            if (string.IsNullOrEmpty(Method))
            {
                Request.Method = "GET";
            }
            else
            {
                Request.Method = Method;
            }
            Request.CookieContainer = CookieContainer ?? WebClientService.CookieContainer;
            foreach (var clientCertificate in ClientCertificates)
            {
                Request.ClientCertificates.Add(clientCertificate);
            }
            Request.UserAgent = UserAgent ?? (string.IsNullOrEmpty(UserAgentVersion) ? WebClientService.GetUserAgent(null) : WebClientService.GetUserAgent(UserAgentVersion));
            Request.KeepAlive = KeepAlive;
            Request.ContentType = ContentType;
            if (HasSupportGZip)
            {
                Request.AutomaticDecompression = DecompressionMethods.GZip;
                Request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
            }
            foreach (var headerByHttpRequestHeader in HeadersByHttpRequestHeader)
            {
                Request.Headers.Add(headerByHttpRequestHeader.Key, headerByHttpRequestHeader.Value);
            }
            foreach (var headerByString in HeadersByString)
            {
                Request.Headers.Add(headerByString.Key, headerByString.Value);
            }
            foreach (var header in Headers)
            {
                Request.Headers.Add(header);
            }
        }

        public IWebClientRequest Get() => SetMethod("GET");

        public IWebClientRequest Post() => SetMethod("POST");

        public IWebClientRequest Put() => SetMethod("PUT");

        public IWebClientRequest Delete() => SetMethod("DELETE");

        public IWebClientRequest Option() => SetMethod("OPTION");

        public IWebClientRequest SetMethod(string method)
        {
            Method = method;
            return this;
        }

        public IWebClientRequest SetContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }

        public IWebClientRequest SetUserAgent(string userAgent)
        {
            UserAgent = userAgent;
            return this;
        }

        public IWebClientRequest SetUserAgentVersion(string userAgentVersion)
        {
            UserAgentVersion = userAgentVersion;
            return this;
        }

        public IWebClientRequest SetPostData(string value) => SetPostData(Encoding.UTF8.GetBytes(value));

        public IWebClientRequest SetPostData(byte[] value)
        {
            PostData = value;
            return this;
        }

        public IWebClientRequest SetPostFilename(string filename) => SetPostData(File.ReadAllBytes(filename));

        public IWebClientRequest SetPostData(JObject value) => SetPostData(value.ToString(Newtonsoft.Json.Formatting.None));

        public IWebClientRequestPostData StartPostData() => new WebClientRequestPostData(this);

        public IWebClientRequest SetGetData(string key, string value)
        {
            GetParameters.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public IWebClientRequest SetGetData(IEnumerable<KeyValuePair<string, string>> dict)
        {
            foreach (var item in dict)
            {
                SetGetData(item.Key, item.Value);
            }
            return this;
        }

        public IWebClientRequest AddHeader(HttpRequestHeader httpRequestHeader, string value)
        {
            HeadersByHttpRequestHeader.Add(new KeyValuePair<HttpRequestHeader, string>(httpRequestHeader, value));
            return this;
        }

        public IWebClientRequest AddHeader(string header, string value)
        {
            HeadersByString.Add(new KeyValuePair<string, string>(header, value));
            return this;
        }

        public IWebClientRequest AddHeader(string value)
        {
            Headers.Add(value);
            return this;
        }

        public IWebClientRequest UseCookieContainer(CookieContainer cookieContainer)
        {
            CookieContainer = cookieContainer;

            return this;
        }

        public IWebClientRequest UseClientCertificate(X509Certificate2 clientCertificate)
        {
            ClientCertificates.Add(clientCertificate);

            return this;
        }

        public IWebClientRequest SupportGZip(bool support)
        {
            HasSupportGZip = support;
            return this;
        }

        private HttpWebResponse Response { get; set; }

        private HttpWebResponse GetResponse()
        {
            if (Response == null)
            {
                HttpWebResponse webResponse = null;
                try
                {
                    EnsureParametersSetBeforeCall();

                    byte[] data = PostData;

                    if (data != null)
                    {
                        Stream requestStream = null;
                        Request.ContentLength = data.Length;
                        try
                        {
                            requestStream = WebClientService.GetRequestStreamWithTimeout(Request, Timeout);
                        }
                        catch (Exception e)
                        {
                            Request.Abort();
                            throw new WebClientException("Unable to get response from server", e);
                        }

                        try
                        {
                            WebClientService.ExecSyncWithTimeout
                            (
                                (callback, state) => requestStream.BeginWrite(data, 0, data.Length, callback, state),
                                requestStream.EndWrite,
                                Timeout
                            );
                        }
                        catch (Exception e)
                        {
                            Request.Abort();
                            throw new WebClientException("Unable to send data to server", e);
                        }

                        requestStream.Close();
                    }

                    webResponse = WebClientService.GetResponseWithTimeout(Request, Timeout);
                }
                catch (Exception e)
                {
                    Request.Abort();
                    throw new WebClientException("Unable to get response from server", e);
                }

                if (!IsAcceptedStatusCode(webResponse.StatusCode))
                {
                    throw new WebClientException(string.Format("The server did not respond with accepted status code, but with [{0}]", webResponse.StatusCode));
                }
                Response = webResponse;
            }
            return Response;
        }

        private bool IsAcceptedStatusCode(HttpStatusCode statusCode)
        {
            if (RejectedStatusCode.Contains(statusCode))
            {
                return false;
            }
            if (AcceptedStatusCode == null)
            {
                return true;
            }
            if (AcceptedStatusCode.Contains(statusCode))
            {
                return true;
            }
            return false;
        }

        public HttpStatusCode GetStatusCode() => GetResponse().StatusCode;

        public string GetAsString() => Encoding.UTF8.GetString(GetAsByteArray());

        private void CopyStream(Stream streamSource, Stream streamDestination)
        {
            int chunkSize = 100000;
            var buffer = new byte[chunkSize];
            int numread = -1;
            int offset = 0;

            while (numread != 0)
            {
                numread = streamSource.Read(buffer, offset, chunkSize);
                streamDestination.Write(buffer, offset, numread);
            }
        }

        public byte[] GetAsByteArray()
        {
            var response = GetResponse();
            try
            {
                using (var fileStream = new MemoryStream())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        var stream = WebClientService.GetTimeoutStream(responseStream, Timeout);

                        CopyStream(stream, fileStream);
                    }
                    return fileStream.ToArray();
                }
            }
            catch (AsyncTimeoutException e)
            {
                throw new WebClientException("Master timeout exception", e);
            }
            catch (Exception e)
            {
                throw new WebClientException("Unable to read response from server", e);
            }
        }

        public void GetAsFile(string filename, FileMode fileMode, FileAccess fileAccess)
        {
            var response = GetResponse();
            try
            {
                using (var fileStream = new FileStream(filename, fileMode, fileAccess))
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        var stream = WebClientService.GetTimeoutStream(responseStream, Timeout);

                        CopyStream(stream, fileStream);
                    }
                }
            }
            catch (AsyncTimeoutException e)
            {
                throw new WebClientException("Master timeout exception", e);
            }
            catch (Exception e)
            {
                throw new WebClientException("Unable to read response from server", e);
            }
        }

        public void Dispose()
        {
            try
            {
                var response = GetResponse();
                if (response != null)
                {
                    response.Close();
                }
            }
            catch (Exception e)
            {
                throw new WebClientException("Problem when disposing WebClientRequest", e);
            }
        }

        public IWebClientRequest Accept(HttpStatusCode statusCode)
        {
            if (AcceptedStatusCode == null)
            {
                AcceptedStatusCode = new HashSet<HttpStatusCode>();
            }
            AcceptedStatusCode.Add(statusCode);
            return this;
        }

        public IWebClientRequest Reject(HttpStatusCode statusCode)
        {
            RejectedStatusCode.Add(statusCode);
            return this;
        }

        public IWebClientRequest Accept(params HttpStatusCode[] statusCodes)
        {
            foreach (var statusCode in statusCodes)
            {
                Accept(statusCode);
            }
            return this;
        }

        public IWebClientRequest Reject(params HttpStatusCode[] statusCodes)
        {
            foreach (var statusCode in statusCodes)
            {
                Reject(statusCode);
            }
            return this;
        }
    }
}