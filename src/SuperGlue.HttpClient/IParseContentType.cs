using System.Collections.Generic;
using System.Net.Http;

namespace SuperGlue.HttpClient
{
    public interface IParseContentType
    {
        bool Matches(string contentType);
        HttpContent GetContent(IReadOnlyDictionary<string, object> parameters);
        T ParseResponse<T>(string body);
    }
}