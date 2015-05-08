using System.Reflection;
using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    public class ExecuteZeroModelInOneModelOutEndpoint<TEndpoint, TOutput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteZeroModelInOneModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IOpenWebContext openWebContext)
        {
            TOutput result;

            if (endpointMethod.IsAsyncMethod())
                result = await (Task<TOutput>)endpointMethod.Invoke(_endpoint, new object[0]);
            else
                result = (TOutput)endpointMethod.Invoke(_endpoint, new object[0]);

            openWebContext.Set(result);
        }
    }
}