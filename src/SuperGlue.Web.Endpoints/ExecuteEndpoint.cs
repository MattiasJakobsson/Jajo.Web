using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteEndpoint
    {
        private readonly AppFunc _next;
        private readonly Cache<Type, Func<object, object, IDictionary<string, object>, Task>> _endpointExecutionMethods = new Cache<Type, Func<object, object, IDictionary<string, object>, Task>>();

        public ExecuteEndpoint(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routeInformation = environment.GetRouteInformation();

            if (routeInformation.RoutedTo != null)
            {
                var executor = environment.Resolve(typeof(IExecuteTypeOfEndpoint<>).MakeGenericType(routeInformation.RoutedTo.GetType()));

                if (executor != null)
                {
                    var executionMethod = _endpointExecutionMethods.Get(routeInformation.RoutedTo.GetType(), key => CompileExecutionFunctionFor(executor.GetType(), routeInformation.RoutedTo.GetType()));

                    await executionMethod(executor, routeInformation.RoutedTo, environment);
                }
            }

            await _next(environment);
        }

        protected Func<object, object, IDictionary<string, object>, Task> CompileExecutionFunctionFor(Type executorType, Type routedToType)
        {
            return (Func<object, object, IDictionary<string, object>, Task>)GetType()
                .GetMethod("CompileExecutionFunctionForGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(executorType, routedToType)
                .Invoke(this, new object[0]);
        }

        protected Func<object, object, IDictionary<string, object>, Task> CompileExecutionFunctionForGeneric<TExecutor, TRoutedTo>()
        {
            var executorType = typeof(TExecutor);
            var routedToType = typeof(TRoutedTo);

            var method = executorType.GetMethod("Execute", new[] { routedToType, typeof(IDictionary<string, object>) });

            var executorParameter = Expression.Parameter(executorType);
            var endpointParameter = Expression.Parameter(routedToType);
            var environmentParameter = Expression.Parameter(typeof(IDictionary<string, object>));

            var execute = Expression
                .Lambda<Func<TExecutor, TRoutedTo, IDictionary<string, object>, Task>>(Expression.Call(executorParameter, method, endpointParameter, environmentParameter), executorParameter, endpointParameter, environmentParameter)
                .Compile();

            return ((executor, routedTo, environment) => execute((TExecutor)executor, (TRoutedTo)routedTo, environment));
        }
    }
}