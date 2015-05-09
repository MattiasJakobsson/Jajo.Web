using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            return new RoutingData(new ReadOnlyDictionary<string, object>(environment.Get<IDictionary<string, object>>("route.Parameters")), environment.Get<object>("route.RoutedTo"));
        }

        public static TService Resolve<TService>(this IDictionary<string, object> environment)
        {
            return (TService) Resolve(environment, typeof (TService));
        }

        public static object Resolve(this IDictionary<string, object> environment, Type serviceType)
        {
            return environment.Get<Func<Type, object>>("openweb.ResolveInstance")(serviceType);
        }

        public static IEnumerable<TService> ResolveAll<TService>(this IDictionary<string, object> environment)
        {
            return ResolveAll(environment, typeof (TService)).OfType<TService>();
        }

        public static IEnumerable<object> ResolveAll(this IDictionary<string, object> environment, Type serviceType)
        {
            return environment.Get<Func<Type, IEnumerable<object>>>("openweb.ResolveAllInstances")(serviceType);
        }

        public static void ConfigureResolvers(this IDictionary<string, object> environment, Func<Type, object> resolve, Func<Type, IEnumerable<object>> resolveAll)
        {
            environment["openweb.ResolveInstance"] = resolve;
            environment["openweb.ResolveAllInstances"] = resolveAll;
        }

        public static void SetModelBinder(this IDictionary<string, object> environment, Func<Type, object> binder)
        {
            environment["openweb.ModelBinder"] = binder;
        }

        public static Task WriteToOutput(this IDictionary<string, object> environment, Stream data)
        {
            data.Position = 0;

            return data.CopyToAsync(environment.Get<Stream>("owin.ResponseBody"));
        }

        public static async Task WriteToOutput(this IDictionary<string, object> environment, string content)
        {
            var textWriter = new StreamWriter(environment.Get<Stream>("owin.ResponseBody"));

            await textWriter.WriteAsync(content);
            await textWriter.FlushAsync();
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
            var modelBinder = environment.Get<Func<Type, object>>("openweb.ModelBinder");
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
            public RoutingData(IReadOnlyDictionary<string, object> parameters, object routedTo)
            {
                Parameters = parameters;
                RoutedTo = routedTo;
            }

            public IReadOnlyDictionary<string, object> Parameters { get; private set; }
            public object RoutedTo { get; private set; }
        }
    }
}