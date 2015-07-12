using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public static class ApiExtensions
    {
        public static async Task<IHttpResponse> ExecuteFormAt(this IApiClient api, ApiDefinition apiDefiniation, IDictionary<string, object> data, IFormTravelInstruction travelInstruction, params IApiLinkTravelInstruction[] instructions)
        {
            var resource = await api.TravelTo(apiDefiniation, data, instructions);

            if (resource == null)
                return null;

            return await api.ExecuteForm(resource, data, travelInstruction);
        }
    }
}