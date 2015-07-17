using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public interface IHttpResponse
    {
        Task<string> ReadRawBody();
        Task<T> ReadBodyAs<T>();
        HttpResponseHeaders Headers { get; }
        HttpStatusCode StatusCode { get; }
        bool IsSuccessStatusCode { get; }
    }
}