using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public interface IHttpClient
    {
        Task<TResponse> Get<TResponse>(string url, object query = null);
        Task<TResponse> Post<TResponse>(string url, object data, string contentType, IDictionary<string, object> environment);
    }
}