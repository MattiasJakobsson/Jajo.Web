using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public class ExecuteZeroModelInOneModelOutEndpoint<TEndpoint, TOutput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        private static readonly Cache<MethodInfo, Func<TEndpoint, Task<TOutput>>> ExecutionMethodsCache = new Cache<MethodInfo, Func<TEndpoint, Task<TOutput>>>(
            x =>
            {
                var endpointParameter = Expression.Parameter(typeof(TEndpoint));

                if (x.IsAsyncMethod())
                    return Expression.Lambda<Func<TEndpoint, Task<TOutput>>>(Expression.Call(endpointParameter, x), endpointParameter).Compile();

                var lambda = Expression.Lambda<Func<TEndpoint, TOutput>>(Expression.Call(endpointParameter, x), endpointParameter).Compile();

                return (endpoint => Task.Factory.StartNew(() => lambda(endpoint)));
            });

        public ExecuteZeroModelInOneModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IDictionary<string, object> environment)
        {
            var result = await ExecutionMethodsCache[endpointMethod](_endpoint);

            environment.SetOutput(result);
        }
    }
}