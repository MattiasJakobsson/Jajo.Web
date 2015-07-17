using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public class DefaultHttpResponse : IHttpResponse
    {
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly IEnumerable<IParseContentType> _contentTypeParsers;

        public DefaultHttpResponse(HttpResponseMessage httpResponseMessage, IEnumerable<IParseContentType> contentTypeParsers)
        {
            _httpResponseMessage = httpResponseMessage;
            _contentTypeParsers = contentTypeParsers;
        }

        public Task<string> ReadRawBody()
        {
            return _httpResponseMessage.Content.ReadAsStringAsync();
        }

        public async Task<T> ReadBodyAs<T>()
        {
            var parser = _contentTypeParsers.FirstOrDefault(x => x.Matches(_httpResponseMessage.Content.Headers.ContentType.MediaType));

            return parser != null ? parser.ParseResponse<T>(await ReadRawBody()) : default(T);
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