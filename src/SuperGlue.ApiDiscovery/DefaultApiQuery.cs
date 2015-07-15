using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public class DefaultApiQuery : IApiQuery
    {
        private readonly Func<Task<ApiDefinition>> _findDefinition;
        private readonly IExecuteApiRequests _executeApiRequests;
        private readonly ICollection<IApiLinkTravelInstruction> _instructions = new List<IApiLinkTravelInstruction>();

        public DefaultApiQuery(Func<Task<ApiDefinition>> findDefinition, IExecuteApiRequests executeApiRequests)
        {
            _findDefinition = findDefinition;
            _executeApiRequests = executeApiRequests;
        }

        public IApiQuery TravelTo(IApiLinkTravelInstruction travelInstruction)
        {
            _instructions.Add(travelInstruction);

            return this;
        }

        public async Task<IApiResource> Query(IDictionary<string, object> data)
        {
            var definition = await _findDefinition();

            return await _executeApiRequests.ExecuteQuery(definition, _instructions, data);
        }

        public async Task<IHttpResponse> ExecuteForm(IFormTravelInstruction travelToForm, IDictionary<string, object> data)
        {
            var resource = await Query(data);

            return await _executeApiRequests.ExecuteForm(resource, travelToForm, data);
        }
    }
}