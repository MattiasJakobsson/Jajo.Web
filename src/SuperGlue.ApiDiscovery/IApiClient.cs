using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiClient
    {
        Task<IApiResource> TravelTo(ApiDefinition api, IDictionary<string, object> data, params IApiLinkTravelInstruction[] instructions);
        Task<IHttpResponse> ExecuteForm(IApiResource resource, IDictionary<string, object> data, IFormTravelInstruction travelInstruction);
    }
}