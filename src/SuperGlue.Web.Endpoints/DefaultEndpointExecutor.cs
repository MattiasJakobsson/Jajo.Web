using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public class DefaultEndpointExecutor : IExecuteAnyEndpoint
    {
        private static readonly Cache<Type, Func<object, object, IDictionary<string, object>, Task>> EndpointExecutionMethods = new Cache<Type, Func<object, object, IDictionary<string, object>, Task>>();

        public virtual async Task Execute(IDictionary<string, object> environment, object endpoint)
        {
            var executor = environment.Resolve(typeof(IExecuteTypeOfEndpoint<>).MakeGenericType(endpoint.GetType()));

            if (executor != null)
            {
                var executionMethod = FindExecutionMethod(executor, endpoint);

                await ExecuteMethod(executionMethod, executor, endpoint, environment).ConfigureAwait(false);
            }
        }

        protected virtual Func<object, object, IDictionary<string, object>, Task> FindExecutionMethod(object executor, object endpoint)
        {
            return EndpointExecutionMethods.Get(endpoint.GetType(), key => CompileExecutionFunctionFor(executor.GetType(), endpoint.GetType()));
        }

        protected virtual Task ExecuteMethod(Func<object, object, IDictionary<string, object>, Task> method, object executor, object endpoint, IDictionary<string, object> environment)
        {
            return method(executor, endpoint, environment);
        }

        protected virtual Func<object, object, IDictionary<string, object>, Task> CompileExecutionFunctionFor(Type executorType, Type routedToType)
        {
            return (Func<object, object, IDictionary<string, object>, Task>)GetType()
                .GetMethod("CompileExecutionFunctionForGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(executorType, routedToType)
                .Invoke(this, new object[0]);
        }

        protected virtual Func<object, object, IDictionary<string, object>, Task> CompileExecutionFunctionForGeneric<TExecutor, TRoutedTo>()
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