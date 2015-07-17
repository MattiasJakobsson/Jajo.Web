using System.Collections.Generic;
using System.Net.Http;
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
            return new StringContent(JsonConvert.SerializeObject(parameters));
        }

        public T ParseResponse<T>(string body)
        {
            return JsonConvert.DeserializeObject<T>(body);
        }
    }
}