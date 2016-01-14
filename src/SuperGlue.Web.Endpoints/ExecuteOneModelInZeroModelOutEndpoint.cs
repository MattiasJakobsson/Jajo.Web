using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public class ExecuteOneModelInZeroModelOutEndpoint<TEndpoint, TInput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        private static readonly Cache<MethodInfo, Func<TEndpoint, TInput, Task>> ExecutionMethodsCache = new Cache<MethodInfo, Func<TEndpoint, TInput, Task>>(
            x =>
            {
                var endpointParameter = Expression.Parameter(typeof(TEndpoint));
                var inputParameter = Expression.Parameter(typeof(TInput));

                if (x.IsAsyncMethod())
                    return Expression.Lambda<Func<TEndpoint, TInput, Task>>(Expression.Call(endpointParameter, x, inputParameter), endpointParameter, inputParameter).Compile();

                var lambda = Expression.Lambda<Action<TEndpoint, TInput>>(Expression.Call(endpointParameter, x, inputParameter), endpointParameter, inputParameter).Compile();

                return ((endpoint, input) => Task.Factory.StartNew(() => lambda(endpoint, input)));
            });

        public ExecuteOneModelInZeroModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IDictionary<string, object> environment)
        {
            await ExecutionMethodsCache[endpointMethod](_endpoint, await environment.Bind<TInput>().ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}