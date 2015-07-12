using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.ApiDiscovery
{
    public class DefaultApiRegistry : IApiRegistry
    {
        private readonly IApiClient _api;

        public DefaultApiRegistry(IApiClient api)
        {
            _api = api;
        }

        public Task Register(IDictionary<string, object> environment, params ApiDefinition[] apis)
        {
            var settings = environment.GetSettings<ApiDiscoverySettings>();

            if(settings.RegistrationRoot == null)
                throw new ApiException("No registration root was configured");

            return _api.ExecuteFormAt(settings.RegistrationRoot, new Dictionary<string, object>
            {
                {"Definitions", apis}
            }, new TravelToTopLevelForm("register"), new TravelChildren("self", new ChildSelector("resources", x => x.State.ContainsKey("name") && x.State["name"].Value == "registration")));
        }

        public async Task<ApiDefinition> Find(IDictionary<string, object> environment, string name)
        {
            var settings = environment.GetSettings<ApiDiscoverySettings>();

            if (settings.RegistrationRoot == null)
                throw new ApiException("No registration root was configured");

            var result = await _api.TravelTo(settings.RegistrationRoot, new Dictionary<string, object>
            {
                {"name", name}
            }, new TravelChildren("find", new ChildSelector("resources", x => x.State.ContainsKey("name") && x.State["name"].Value == "registration")));
        
            return new ApiDefinition(result.State["name"].Value, new Uri(result.State["uri"].Value), result.State["accepts"].Value.Split(';'));
        }
    }
}