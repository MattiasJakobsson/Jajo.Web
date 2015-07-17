using System.Threading.Tasks;

namespace SuperGlue.HttpClient
{
    public interface IHttpResponse
    {
        Task<string> ReadRawBody();
        Task<T> ReadBodyAs<T>();
    }
}