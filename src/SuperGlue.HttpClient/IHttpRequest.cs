using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public interface IHttpRequest
    {
        IHttpRequest ModifyHeaders(Action<HttpRequestHeaders> modifier);
        IHttpRequest Method(string method);
        IHttpRequest ContentType(string contentType);
        IHttpRequest Parameter(string key, object value);

        Task<IHttpResponse> Send();
    }
}