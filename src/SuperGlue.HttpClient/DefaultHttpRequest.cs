using System;
using System.Collections.Generic;
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
        private string _method = "GET";
        private readonly IDictionary<string, string> _parameters = new Dictionary<string, string>();
        private readonly ICollection<Action<HttpRequestHeaders>> _headerModifiers = new List<Action<HttpRequestHeaders>>();

        public DefaultHttpRequest(Uri url)
        {
            _url = url;
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
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            return this;
        }

        public IHttpRequest Parameter(string key, object value)
        {
            _parameters[key] = (value ?? "").ToString();
            return this;
        }

        public async Task<IHttpResponse> Send()
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(_method), _url);

            foreach (var modifier in _headerModifiers)
                modifier(requestMessage.Headers);

            requestMessage.Content = new FormUrlEncodedContent(_parameters);

            var response = await HttpClient.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            return new DefaultHttpResponse(response);
        }
    }
}