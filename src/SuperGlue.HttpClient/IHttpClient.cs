using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public interface IHttpClient
    {
        IHttpRequest Start(string url);
    }

    public interface IHttpRequest
    {
        IHttpRequest Header(string key, string value);
        IHttpRequest Method(string method);
        IHttpRequest ContentType(string contentType);
        IHttpRequest Parameter(string key, object value);

        Task<IHttpResponse> Send();
    }

    public interface IHttpResponse
    {
        string RawBody { get; }
        T BodyAs<T>();
    }
}