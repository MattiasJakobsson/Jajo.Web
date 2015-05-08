using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    public class WebEngine : IWebEngine
    {
        public async Task ExecuteRequest(IRequest request)
        {
            var executor = request.RoutedTo.GetCorrectEndpointExecutor(request);

            if (executor == null)
                return;

            await executor.Execute(request.RoutedTo, request);
        }
    }

    public interface IExecuteEndpoint
    {
        Task Execute(MethodInfo endpointMethod, IRequest request);
    }

    public static class EndpointExtensions
    {
        public static IExecuteEndpoint GetCorrectEndpointExecutor(this MethodInfo endpoint, IRequest request)
        {
            var endpointType = GetCorrectEndpointExecutorType(endpoint);

            return request.Resolve(endpointType) as IExecuteEndpoint;
        }

        public static bool IsAsyncMethod(this MethodInfo method)
        {
            return typeof(Task).IsAssignableFrom(method.ReturnType);
        }

        private static Type GetCorrectEndpointExecutorType(MethodInfo endpoint)
        {
            var inputType = endpoint.GetParameters().Select(x => x.ParameterType).FirstOrDefault();
            var outputType = GetReturnType(endpoint);

            if (inputType != null)
            {
                if (outputType != typeof(void))
                    return typeof(ExecuteOneModelInOneModelOutEndpoint<,,>).MakeGenericType(endpoint.ReflectedType, inputType, outputType);

                return typeof(ExecuteOneModelInZeroModelOutEndpoint<,>).MakeGenericType(endpoint.ReflectedType, inputType);
            }

            if (outputType != typeof(void))
                return typeof(ExecuteZeroModelInOneModelOutEndpoint<,>).MakeGenericType(endpoint.ReflectedType, outputType);

            return typeof(ExecuteZeroModelInZeroModelOutEndpoint<>).MakeGenericType(endpoint.ReflectedType);
        }

        private static Type GetReturnType(MethodInfo method)
        {
            var output = method.ReturnType;

            if (!method.IsAsyncMethod()) 
                return output;
            
            if (output.GenericTypeArguments.Length == 1 && typeof(Task<>).MakeGenericType(output.GenericTypeArguments[0]) == output)
                return output.GenericTypeArguments[0];

            return typeof (void);
        }
    }

    public class ExecuteOneModelInOneModelOutEndpoint<TEndpoint, TInput, TOutput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteOneModelInOneModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IRequest request)
        {
            TOutput result;

            if (endpointMethod.IsAsyncMethod())
                result = await (Task<TOutput>)endpointMethod.Invoke(_endpoint, new object[] { request.Get<TInput>() });
            else
                result = (TOutput)endpointMethod.Invoke(_endpoint, new object[] { request.Get<TInput>() });

            request.Set(result);
        }
    }

    public class ExecuteOneModelInZeroModelOutEndpoint<TEndpoint, TInput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteOneModelInZeroModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IRequest request)
        {
            if (endpointMethod.IsAsyncMethod())
                await (Task)endpointMethod.Invoke(_endpoint, new object[] { request.Get<TInput>() });
            else
                endpointMethod.Invoke(_endpoint, new object[] { request.Get<TInput>() });
        }
    }

    public class ExecuteZeroModelInOneModelOutEndpoint<TEndpoint, TOutput> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteZeroModelInOneModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IRequest request)
        {
            TOutput result;

            if (endpointMethod.IsAsyncMethod())
                result = await (Task<TOutput>)endpointMethod.Invoke(_endpoint, new object[0]);
            else
                result = (TOutput)endpointMethod.Invoke(_endpoint, new object[0]);

            request.Set(result);
        }
    }

    public class ExecuteZeroModelInZeroModelOutEndpoint<TEndpoint> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteZeroModelInZeroModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IRequest request)
        {
            if (endpointMethod.IsAsyncMethod())
                await (Task)endpointMethod.Invoke(_endpoint, new object[0]);
            else
                endpointMethod.Invoke(_endpoint, new object[0]);
        }
    }
}