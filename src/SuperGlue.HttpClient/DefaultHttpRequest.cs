using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public class DefaultHttpRequest : IHttpRequest
    {
        private static readonly System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient(new WebRequestHandler
        {
            CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default),
            AllowAutoRedirect = true
        });

        private readonly Uri _url;
        private readonly IEnumerable<IParseContentType> _contentTypeParsers;
        private string _method = "GET";
        private string _contentType = "application/x-www-form-urlencoded";
        private bool _shouldThrow;
        private readonly IDictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly ICollection<Action<HttpRequestHeaders>> _headerModifiers = new List<Action<HttpRequestHeaders>>();

        public DefaultHttpRequest(Uri url, IEnumerable<IParseContentType> contentTypeParsers)
        {
            _url = url;
            _contentTypeParsers = contentTypeParsers;
        }

        public IHttpRequest ModifyHeaders(Action<HttpRequestHeaders> modifier)
        {
            _headerModifiers.Add(modifier);
            return this;
        }

        public IHttpRequest Method(string method)
        {
            _method = method;
            return this;
        }

        public IHttpRequest ContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public IHttpRequest Parameter(string key, object value)
        {
            _parameters[key] = (value ?? "").ToString();
            return this;
        }

        public IHttpRequest ThrowOnError()
        {
            _shouldThrow = true;

            return this;
        }

        public async Task<IHttpResponse> Send()
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(_method), _url);

            foreach (var modifier in _headerModifiers)
                modifier(requestMessage.Headers);

            requestMessage.Headers.Add("Content-Type", _contentType);

            if (!requestMessage.Method.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) && !requestMessage.Method.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
            {
                var parser = _contentTypeParsers.FirstOrDefault(x => x.Matches(_contentType));

                requestMessage.Content = parser != null ? parser.GetContent(new ReadOnlyDictionary<string, object>(_parameters)) : new FormUrlEncodedContent(_parameters.ToDictionary(x => x.Key, x => (x.Value ?? "").ToString()));
            }

            var response = await HttpClient.SendAsync(requestMessage);

            if (_shouldThrow)
                response.EnsureSuccessStatusCode();

            return new DefaultHttpResponse(response, _contentTypeParsers);
        }
    }
}