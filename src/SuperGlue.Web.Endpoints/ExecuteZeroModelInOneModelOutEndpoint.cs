using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public class ExecuteZeroModelInOneModelOutEndpoint<TEndpoint, TOutput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteZeroModelInOneModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IDictionary<string, object> environment)
        {
            TOutput result;

            if (endpointMethod.IsAsyncMethod())
                result = await (Task<TOutput>)endpointMethod.Invoke(_endpoint, new object[0]);
            else
                result = (TOutput)endpointMethod.Invoke(_endpoint, new object[0]);

            environment.SetOutput(result);
        }
    }
}