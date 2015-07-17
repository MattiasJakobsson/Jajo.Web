using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperGlue.HttpClient
{
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

        public async Task<T> ReadBodyAs<T>()
        {
            return JsonConvert.DeserializeObject<T>(await ReadRawBody());
        }

        public HttpResponseHeaders Headers
        {
            get { return _httpResponseMessage.Headers; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _httpResponseMessage.StatusCode; }
        }

        public bool IsSuccessStatusCode
        {
            get { return _httpResponseMessage.IsSuccessStatusCode; }
        }
    }
}