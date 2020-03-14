using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Strev.WebClient.Service
{
    public class WebClientRequestPostData : IWebClientRequestPostData
    {
        private WebClientRequest Request { get; set; }
        private List<KeyValuePair<string, string>> Data { get; } = new List<KeyValuePair<string, string>>();

        public WebClientRequestPostData(WebClientRequest request)
        {
            Request = request;
        }

        public IWebClientRequestPostData Add(string key, string value)
        {
            Data.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public IWebClientRequestPostData Add(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            foreach (var pair in pairs)
            {
                Data.Add(new KeyValuePair<string, string>(pair.Key, pair.Value));
            }
            return this;
        }

        public IWebClientRequest AsJson()
        {
            var jObject = new JObject();
            foreach (var keyValuePair in Data)
            {
                jObject.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return Request.SetPostData(jObject);
        }

        public IWebClientRequest AsHtml()
        {
            return AsHtml(true);
        }

        public IWebClientRequest AsRawHtml()
        {
            return AsHtml(false);
        }

        public IWebClientRequest AsHtml(bool urlEncode)
        {
            var postDataBuilder = new StringBuilder();
            var first = true;

            foreach (var keyValuePair in Data)
            {
                if (!first)
                {
                    postDataBuilder.Append("&");
                }
                first = false;
                postDataBuilder.Append(string.Format("{0}={1}", keyValuePair.Key, urlEncode ? HttpUtility.UrlEncode(keyValuePair.Value) : keyValuePair.Value));
            }

            Request.SetPostData(postDataBuilder.ToString());
            return Request;
        }
    }
}