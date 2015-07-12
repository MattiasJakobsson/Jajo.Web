using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public interface IParseApiResponse
    {
        string ContentType { get; }
        IApiResource Parse(IHttpResponse response);
    }
}