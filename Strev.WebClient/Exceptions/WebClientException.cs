using System;

namespace Strev.WebClient.Exceptions
{
    [Serializable]
    public class WebClientException : Exception
    {
        public WebClientException()
        {
        }

        public WebClientException(string message) : base(message)
        {
        }

        public WebClientException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}