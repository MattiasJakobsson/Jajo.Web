using System;
using System.Collections.Generic;

namespace Jajo.Web
{
    internal static class BindExtensions
    {
        public static T Bind<T>(this IDictionary<string, object> environment)
        {
            var modelBinder = environment.Get<Func<Type, object>>("jajo.ModelBinder");
            var requestTypedParameters = GetRequestTypedParameters(environment);

            return requestTypedParameters.ContainsKey(typeof(T)) ? (T)requestTypedParameters[typeof(T)] : (T)modelBinder(typeof(T));
        }

        public static void Set<T>(this IDictionary<string, object> environment, T data)
        {
            var requestTypedParameters = GetRequestTypedParameters(environment);

            requestTypedParameters[typeof(T)] = data;
        }

        private static IDictionary<Type, object> GetRequestTypedParameters(IDictionary<string, object> environment)
        {
            var requestTypedParameters = environment.Get<IDictionary<Type, object>>("jajo.RequestTypedParameters");

            if (requestTypedParameters != null) return requestTypedParameters;

            requestTypedParameters = new Dictionary<Type, object>();
            environment["jajo.RequestTypedParameters"] = requestTypedParameters;

            return requestTypedParameters;
        }
    }
}