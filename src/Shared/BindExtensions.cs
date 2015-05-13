using System;
using System.Collections.Generic;

namespace SuperGlue.Web
{
    internal static class BindExtensions
    {
        public static T Bind<T>(this IDictionary<string, object> environment)
        {
            return (T) environment.Bind(typeof (T));
        }

        public static object Bind(this IDictionary<string, object> environment, Type type)
        {
            var modelBinder = environment.Get<Func<Type, object>>("superglue.ModelBinder");
            var requestTypedParameters = GetRequestTypedParameters(environment);

            return requestTypedParameters.ContainsKey(type) ? requestTypedParameters[type] : modelBinder(type);
        }

        public static void Set<T>(this IDictionary<string, object> environment, T data)
        {
            var requestTypedParameters = GetRequestTypedParameters(environment);

            requestTypedParameters[typeof(T)] = data;
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