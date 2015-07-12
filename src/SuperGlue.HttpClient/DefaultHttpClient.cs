using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public class DefaultHttpClient : IHttpClient
    {
        public Task<TResponse> Get<TResponse>(string url, object query = null)
        {
            var client = new EasyHttp.Http.HttpClient();

            var response = client.Get(url, query);

            return Task.FromResult(response.StaticBody<TResponse>());
        }

        public Task<TResponse> Post<TResponse>(string url, object data, string contentType, IDictionary<string, object> environment)
        {
            var client = new EasyHttp.Http.HttpClient();

            var response = client.Post(url, data, contentType);

            return Task.FromResult(response.StaticBody<TResponse>());
        }

        public IHttpRequest Start(string url)
        {
            return null;
        }
    }
}