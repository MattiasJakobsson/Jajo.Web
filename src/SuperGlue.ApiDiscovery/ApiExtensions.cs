using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.ApiDiscovery
{
    public static class ApiExtensions
    {
        public static async Task<string> ExecuteFormAt(this IApi api, ApiDefinition apiDefiniation, IDictionary<string, object> data, IFormTravelInstruction travelInstruction, params IApiLinkTravelInstruction[] instructions)
        {
            var resource = await api.TravelTo(apiDefiniation, data, instructions);

            if (resource == null)
                return "";

            return await api.ExecuteForm(resource, data, travelInstruction);
        }
    }
}