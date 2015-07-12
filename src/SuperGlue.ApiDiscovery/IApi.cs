using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.ApiDiscovery
{
    public interface IApi
    {
        Task<IApiResource> TravelTo(ApiDefinition api, IDictionary<string, object> data, params IApiLinkTravelInstruction[] instructions);
        Task<string> ExecuteForm(IApiResource resource, IDictionary<string, object> data, IFormTravelInstruction travelInstruction);
    }
}