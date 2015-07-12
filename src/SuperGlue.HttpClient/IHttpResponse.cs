using System.Net.Http;
using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public interface IHttpResponse
    {
        Task<string> ReadRawBody();
        Task<T> ReadBodyAs<T>();
    }

    public class DefaultHttpResponse : IHttpResponse
    {
        private readonly HttpResponseMessage _httpResponseMessage;

        public DefaultHttpResponse(HttpResponseMessage httpResponseMessage)
        {
            _httpResponseMessage = httpResponseMessage;
        }

        public Task<string> ReadRawBody()
        {
            return _httpResponseMessage.Content.ReadAsStringAsync();
        }

        public Task<T> ReadBodyAs<T>()
        {
            throw new System.NotImplementedException();
        }
    }
}