using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public class ExecuteOneModelInOneModelOutEndpoint<TEndpoint, TInput, TOutput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteOneModelInOneModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IDictionary<string, object> environment)
        {
            TOutput result;

            if (endpointMethod.IsAsyncMethod())
                result = await (Task<TOutput>)endpointMethod.Invoke(_endpoint, new object[] { await environment.Bind<TInput>() });
            else
                result = (TOutput)endpointMethod.Invoke(_endpoint, new object[] { await environment.Bind<TInput>() });

            environment.SetOutput(result);
        }
    }
}