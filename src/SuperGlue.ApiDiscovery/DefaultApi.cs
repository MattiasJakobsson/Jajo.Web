using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.ApiDiscovery
{
    public class DefaultApi : IApi
    {
        private readonly IEnumerable<IHandleApi> _apiHandlers;

        public DefaultApi(IEnumerable<IHandleApi> apiHandlers)
        {
            _apiHandlers = apiHandlers;
        }

        public Task<IApiResource> TravelTo(ApiDefinition api, IDictionary<string, object> data, params IApiLinkTravelInstruction[] instructions)
        {
            var handler = _apiHandlers.FirstOrDefault(x => x.Accepts(api));

            if(handler == null)
                throw new ApiException(string.Format("Can't handle api named: {0}", api.Name));

            return handler.Travel(api, data, instructions);
        }

        public Task<string> ExecuteForm(IApiResource resource, IDictionary<string, object> data, IFormTravelInstruction travelInstruction)
        {
            var handler = _apiHandlers.FirstOrDefault(x => x.Accepts(resource.Definition));

            if (handler == null)
                throw new ApiException(string.Format("Can't handle api named: {0}", resource.Definition.Name));

            return handler.ExecuteForm(resource, data, travelInstruction);
        }
    }
}