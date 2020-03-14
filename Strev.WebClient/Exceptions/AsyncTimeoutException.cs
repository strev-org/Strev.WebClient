using System;
using System.Runtime.Serialization;

namespace Strev.WebClient.Exceptions
{
    [Serializable]
    public class AsyncTimeoutException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AsyncTimeoutException()
        {
        }

        public AsyncTimeoutException(string message) : base(message)
        {
        }

        public AsyncTimeoutException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AsyncTimeoutException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }
}