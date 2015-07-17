using System.Threading.Tasks;
using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public interface IParseApiResponse
    {
        string ContentType { get; }
        Task<ApiResource> Parse(IHttpResponse response, ApiDefinition definition);
    }
}