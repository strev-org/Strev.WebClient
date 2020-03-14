using System;
using System.Net;

namespace Strev.WebClient.Exceptions
{
    [Serializable]
    public class WebClientHttpStatusException : WebClientException
    {
        public HttpStatusCode Code { get; set; }

        public WebClientHttpStatusException()
        {
        }

        public WebClientHttpStatusException(string message) : base(message)
        {
        }

        public WebClientHttpStatusException(string message, Exception inner) : base(message, inner)
        {
        }

        public WebClientHttpStatusException(HttpStatusCode code)
        {
            Code = code;
        }

        public WebClientHttpStatusException(string message, HttpStatusCode code) : base(message)
        {
            Code = code;
        }

        public WebClientHttpStatusException(string message, Exception inner, HttpStatusCode code) : base(message, inner)
        {
            Code = code;
        }
    }
}