using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Jajo.Web.Endpoints
{
    public static class EndpointExtensions
    {
        public static IExecuteEndpoint GetCorrectEndpointExecutor(this MethodInfo endpoint, IDictionary<string, object> environment)
        {
            var endpointType = GetCorrectEndpointExecutorType(endpoint);

            return environment.Resolve(endpointType) as IExecuteEndpoint;
        }

        public static bool IsAsyncMethod(this MethodInfo method)
        {
            return typeof(Task).IsAssignableFrom(method.ReturnType);
        }

        public static Type GetReturnType(this MethodInfo method)
        {
            var output = method.ReturnType;

            if (!method.IsAsyncMethod())
                return output;

            if (output.GenericTypeArguments.Length == 1 && typeof(Task<>).MakeGenericType(output.GenericTypeArguments[0]) == output)
                return output.GenericTypeArguments[0];

            return typeof(void);
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
    }
}