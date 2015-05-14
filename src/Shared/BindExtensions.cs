using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue
{
    internal static class BindExtensions
    {
        public static async Task<T> Bind<T>(this IDictionary<string, object> environment)
        {
            return (T) (await environment.Bind(typeof (T)));
        }

        public static async Task<object> Bind(this IDictionary<string, object> environment, Type type)
        {
            var modelBinder = environment.Get<Func<Type, Task<object>>>("superglue.ModelBinder");
            var requestTypedParameters = GetRequestTypedParameters(environment);

            return requestTypedParameters.ContainsKey(type) ? requestTypedParameters[type] : await modelBinder(type);
        }

        public static void Set<T>(this IDictionary<string, object> environment, T data)
        {
            environment.Set(typeof (T), data);
        }

        public static void Set(this IDictionary<string, object> environment, Type dataType, object data)
        {
            var requestTypedParameters = GetRequestTypedParameters(environment);

            requestTypedParameters[dataType] = data;
        }

        private static IDictionary<Type, object> GetRequestTypedParameters(IDictionary<string, object> environment)
        {
            var requestTypedParameters = environment.Get<IDictionary<Type, object>>("superglue.RequestTypedParameters");

            if (requestTypedParameters != null) return requestTypedParameters;

            requestTypedParameters = new Dictionary<Type, object>();
            environment["superglue.RequestTypedParameters"] = requestTypedParameters;

            return requestTypedParameters;
        }
    }
}