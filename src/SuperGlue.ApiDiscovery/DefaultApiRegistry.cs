using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.ApiDiscovery
{
    public class DefaultApiRegistry : IApiRegistry
    {
        private readonly IExecuteApiRequests _executeApiRequests;

        public DefaultApiRegistry(IExecuteApiRequests executeApiRequests)
        {
            _executeApiRequests = executeApiRequests;
        }

        public Task Register(IDictionary<string, object> environment, params ApiDefinition[] apis)
        {
            var settings = environment.GetSettings<ApiDiscoverySettings>();

            if(settings.RegistrationRoot == null)
                throw new ApiException("No registration root was configured");

            return StartQuery(() => Task.FromResult(settings.RegistrationRoot))
                .ExecuteForm(new TravelToTopLevelForm("register"), new Dictionary<string, object>
                {
                    {"Resources", apis}
                });
        }

        public IApiQuery Start(IDictionary<string, object> environment, string apiName)
        {
            return StartQuery(() => FindDefinitionFor(environment, apiName));
        }

        protected virtual async Task<ApiDefinition> FindDefinitionFor(IDictionary<string, object> environment, string name)
        {
            var settings = environment.GetSettings<ApiDiscoverySettings>();

            if (settings.RegistrationRoot == null)
                throw new ApiException("No registration root was configured");

            var response = await StartQuery(() => Task.FromResult(settings.RegistrationRoot))
                .TravelTo(new TravelToTopLevelLink("details"))
                .Query(new Dictionary<string, object>
                {
                    {"name", name}
                });

            return response.State;
        }

        protected virtual IApiQuery StartQuery(Func<Task<ApiDefinition>> findDefinition)
        {
            return new DefaultApiQuery(findDefinition, _executeApiRequests);
        }
    }
}