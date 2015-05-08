using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using OpenWeb.ModelBinding;

namespace OpenWeb.Endpoints
{
    public class OpenWebContext : IOpenWebContext
    {
        private readonly WebEnvironment _environment;
        private readonly IModelBinderCollection _modelBinderCollection;

        public OpenWebContext(IDictionary<string, object> environment, IModelBinderCollection modelBinderCollection)
        {
            _modelBinderCollection = modelBinderCollection;
            _environment = new WebEnvironment(environment);
        }

        public IWebEnvironment Environment
        {
            get { return _environment; }
        }

        public MethodInfo RoutedTo
        {
            get { return _environment.Get<MethodInfo>("routing.RoutedTo"); }
        }

        public Stream Body
        {
            get { return _environment.Get<Stream>("owin.ResponseBody"); }
        }

        public T Get<T>()
        {
            var requestTypedParameters = _environment.Get<IDictionary<Type, object>>("openweb.RequestTypedParameters");

            return requestTypedParameters.ContainsKey(typeof(T)) ? (T)requestTypedParameters[typeof(T)] : (T)_modelBinderCollection.Bind(typeof(T), new BindingContext(_modelBinderCollection));
        }

        public void Set<T>(T data)
        {
            var requestTypedParameters = _environment.Get<IDictionary<Type, object>>("openweb.RequestTypedParameters");

            requestTypedParameters[typeof(T)] = data;
        }

        public IResolveDependencies DependencyResolver
        {
            get { return _environment.Get<IResolveDependencies>("openweb.DependencyResolver"); }
            set { _environment.Set("openweb.DependencyResolver", value); }
        }

        private class WebEnvironment : IWebEnvironment
        {
            private readonly IDictionary<string, object> _environment;

            public WebEnvironment(IDictionary<string, object> environment)
            {
                _environment = environment;
            }

            public string Method { get { return Get<string>("owin.RequestMethod"); } }
            public string ContentType { get { return GetHeader(RawHeaders, "Content-Type"); } }
            public IReadOnlyDictionary<string, string[]> RawHeaders { get { return new ReadOnlyDictionary<string, string[]>(Get<IDictionary<string, string[]>>("owin.RequestHeaders")); } }
            public IReadOnlyDictionary<string, object> RouteParameters { get { return new ReadOnlyDictionary<string, object>(Get<IDictionary<string, object>>("route.Parameters")); } }
            public object Output { get { return Get<object>("openweb.Output"); } }

            public T Get<T>(string key)
            {
                object obj;
                if (!_environment.TryGetValue(key, out obj) || !(obj is T))
                    return default(T);

                return (T)obj;
            }

            public void Set(string key, object item)
            {
                _environment[key] = item;
            }

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
    }
}