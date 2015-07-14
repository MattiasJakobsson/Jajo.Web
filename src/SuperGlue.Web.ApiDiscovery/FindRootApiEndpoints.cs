using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.ApiDiscovery;

namespace SuperGlue.Web.ApiDiscovery
{
    public class FindRootApiEndpoints : IApiSource
    {
        private readonly IEnumerable<IRootApiInput> _apiEndpoints;
        private readonly IEnumerable<IRenderApiOutput> _outputRenderers;

        public FindRootApiEndpoints(IEnumerable<IRootApiInput> apiEndpoints, IEnumerable<IRenderApiOutput> outputRenderers)
        {
            _apiEndpoints = apiEndpoints;
            _outputRenderers = outputRenderers;
        }

        public Task<IEnumerable<ApiDefinition>> Find(IDictionary<string, object> environment)
        {
            var accepts = _outputRenderers.Select(x => x.Type).ToList();

            var result = _apiEndpoints.Select(input => new ApiDefinition(input.GetName(environment), new Uri(environment.RouteTo(input)), accepts)).ToList();

            return Task.FromResult<IEnumerable<ApiDefinition>>(result);
        }
    }
}