using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using OpenWeb.ModelBinding;

namespace OpenWeb
{
    public static class EnvironmentExtensions
    {
        public static RequestData GetRequestData(this IDictionary<string, object> environment)
        {
            return new RequestData(environment.Get<string>("owin.RequestMethod"));
        }

        public static HeaderData GetHeaders(this IDictionary<string, object> environment)
        {
            return new HeaderData(new ReadOnlyDictionary<string, string[]>(environment.Get<IDictionary<string, string[]>>("owin.RequestHeaders")));
        }

        public static RoutingData GetRouteInformation(this IDictionary<string, object> environment)
        {
            return new RoutingData(new ReadOnlyDictionary<string, object>(environment.Get<IDictionary<string, object>>("route.Parameters")), environment.Get<MethodInfo>("route.RoutedTo"));
        }

        public static IResolveDependencies GetDependencyResolver(this IDictionary<string, object> environment)
        {
            return environment.Get<IResolveDependencies>("openweb.DependencyResolver");
        }

        public static void SetDependencyResolver(this IDictionary<string, object> environment, IResolveDependencies dependencyResolver)
        {
            environment["openweb.DependencyResolver"] = dependencyResolver;
        }

        public static void SetModelBinder(this IDictionary<string, object> environment, IModelBinderCollection modelBinderCollection)
        {
            environment["openweb.ModelBinder"] = modelBinderCollection;
        }

        public static Task WriteToOutput(this IDictionary<string, object> environment, Stream data)
        {
            data.Position = 0;

            return data.CopyToAsync(environment.Get<Stream>("owin.ResponseBody"));
        }

        public static object GetOutput(this IDictionary<string, object> environment)
        {
            return environment.Get<object>("openweb.Output");
        }

        public static void SetOutput(this IDictionary<string, object> environment, object output)
        {
            environment["openweb.Output"] = output;
        }

        public static T Get<T>(this IDictionary<string, object> environment, string key)
        {
            object obj;
            if (!environment.TryGetValue(key, out obj) || !(obj is T))
                return default(T);

            return (T)obj;
        }

        public static T Bind<T>(this IDictionary<string, object> environment)
        {
            var modelBinder = environment.Get<IModelBinderCollection>("openweb.ModelBinder");
            var requestTypedParameters = GetRequestTypedParameters(environment);

            return requestTypedParameters.ContainsKey(typeof(T)) ? (T)requestTypedParameters[typeof(T)] : (T)modelBinder.Bind(typeof(T), new BindingContext(modelBinder));
        }

        public static void Set<T>(this IDictionary<string, object> environment, T data)
        {
            var requestTypedParameters = GetRequestTypedParameters(environment);

            requestTypedParameters[typeof(T)] = data;
        }

        private static IDictionary<Type, object> GetRequestTypedParameters(IDictionary<string, object> environment)
        {
            var requestTypedParameters = environment.Get<IDictionary<Type, object>>("openweb.RequestTypedParameters");

            if (requestTypedParameters != null) return requestTypedParameters;
            
            requestTypedParameters = new Dictionary<Type, object>();
            environment["openweb.RequestTypedParameters"] = requestTypedParameters;

            return requestTypedParameters;
        }

        public class RequestData
        {
            public RequestData(string method)
            {
                Method = method;
            }

            public string Method { get; private set; }
        }

        public class HeaderData
        {
            public HeaderData(IReadOnlyDictionary<string, string[]> rawHeaders)
            {
                RawHeaders = rawHeaders;
            }

            public IReadOnlyDictionary<string, string[]> RawHeaders { get; private set; }
            public string Accept { get { return GetHeader(RawHeaders, "Accept"); } }

            private static string GetHeader(IReadOnlyDictionary<string, string[]> headers, string key)
            {
                var headerUnmodified = GetHeaderUnmodified(headers, key);
                return headerUnmodified != null ? string.Join(",", headerUnmodified) : null;
            }

            private static string[] GetHeaderUnmodified(IReadOnlyDictionary<string, string[]> headers, string key)
            {
                if (headers == null)
                    throw new ArgumentNullException("headers");

                string[] strArray;

                return !headers.TryGetValue(key, out strArray) ? null : strArray;
            }
        }

        public class RoutingData
        {
            public RoutingData(IReadOnlyDictionary<string, object> parameters, MethodInfo routedTo)
            {
                Parameters = parameters;
                RoutedTo = routedTo;
            }

            public IReadOnlyDictionary<string, object> Parameters { get; private set; }
            public MethodInfo RoutedTo { get; private set; }
        }
    }
}