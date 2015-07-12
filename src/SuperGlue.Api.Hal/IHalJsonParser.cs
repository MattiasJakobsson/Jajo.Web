using SuperGlue.ApiDiscovery;

namespace SuperGlue.Api.Hal
{
    public interface IHalJsonParser
    {
        IApiResource ParseResource(ApiDefinition api, string json);
    }
}