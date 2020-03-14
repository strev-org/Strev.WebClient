using System.Collections.Generic;

namespace Strev.WebClient
{
    public interface IWebClientRequestPostData
    {
        IWebClientRequestPostData Add(string key, string value);

        IWebClientRequestPostData Add(IEnumerable<KeyValuePair<string, string>> pairs);

        /// <summary>
        /// Parameter are json-encoded values
        /// eg: {"key1":"this is value 1","key2":"value2"}
        /// </summary>
        IWebClientRequest AsJson();

        /// <summary>
        /// Parameter are url-encoded ampersand separated values
        /// eg: key1=this%20is%20value%201&key2=value2
        /// </summary>
        IWebClientRequest AsHtml();

        /// <summary>
        /// Parameter are non url-encoded ampersand separated values
        /// eg: key1=this is value 1&key2=value2
        /// </summary>
        IWebClientRequest AsRawHtml();
    }
}