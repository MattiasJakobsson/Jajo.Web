using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public class ExecuteZeroModelInZeroModelOutEndpoint<TEndpoint> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        private static readonly Cache<MethodInfo, Func<TEndpoint, Task>> ExecutionMethodsCache = new Cache<MethodInfo, Func<TEndpoint, Task>>(
            x =>
            {
                var endpointParameter = Expression.Parameter(typeof(TEndpoint));

                if (x.IsAsyncMethod())
                    return Expression.Lambda<Func<TEndpoint, Task>>(Expression.Call(endpointParameter, x), endpointParameter).Compile();

                var lambda = Expression.Lambda<Action<TEndpoint>>(Expression.Call(endpointParameter, x), endpointParameter).Compile();

                return (endpoint => Task.Factory.StartNew(() => lambda(endpoint)));
            });

        public ExecuteZeroModelInZeroModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IDictionary<string, object> environment)
        {
            await ExecutionMethodsCache[endpointMethod](_endpoint);
        }
    }
}