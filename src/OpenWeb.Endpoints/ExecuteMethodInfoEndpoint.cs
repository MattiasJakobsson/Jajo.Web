using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    public class ExecuteMethodInfoEndpoint : IExecuteTypeOfEndpoint<MethodInfo>
    {
        public async Task Execute(MethodInfo endpoint, IDictionary<string, object> environment)
        {
            var executor = endpoint.GetCorrectEndpointExecutor(environment);

            if (executor != null)
                await executor.Execute(endpoint, environment);
        }
    }
}