using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace SuperGlue.HttpClient
{
    public class ParseApplicationJsonContentType : IParseContentType
    {
        public bool Matches(string contentType)
        {
            return contentType.Contains("application/json");
        }

        public HttpContent GetContent(IReadOnlyDictionary<string, object> parameters)
        {
            var content = new StringContent(JsonConvert.SerializeObject(parameters));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }

        public T ParseResponse<T>(string body)
        {
            return JsonConvert.DeserializeObject<T>(body);
        }
    }
}