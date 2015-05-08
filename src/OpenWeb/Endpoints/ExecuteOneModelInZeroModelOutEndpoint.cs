using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    public class ExecuteOneModelInZeroModelOutEndpoint<TEndpoint, TInput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteOneModelInZeroModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IDictionary<string, object> environment)
        {
            if (endpointMethod.IsAsyncMethod())
                await (Task)endpointMethod.Invoke(_endpoint, new object[] { environment.Bind<TInput>() });
            else
                endpointMethod.Invoke(_endpoint, new object[] { environment.Bind<TInput>() });
        }
    }
}