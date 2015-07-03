using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public interface IHttpClient
    {
        Task<TResponse> Post<TResponse>(string url, object data, string contentType, IDictionary<string, object> environment);
    }
}