using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenWeb
{
    public class WebEnvironment
    {
        private readonly IDictionary<string, object> _environment;

        public WebEnvironment(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public string Method { get { return Get<string>("owin.RequestMethod"); } }
        public string ContentType{get { return GetHeader(RawHeaders, "Content-Type"); }}
        public IReadOnlyDictionary<string, string[]> RawHeaders { get { return new ReadOnlyDictionary<string, string[]>(Get<IDictionary<string, string[]>>("owin.RequestHeaders")); } }
        public IReadOnlyDictionary<string, object> RouteParameters { get { return new ReadOnlyDictionary<string, object>(Get<IDictionary<string, object>>("route.Parameters")); } }
        public object Output { get { return Get<object>("openweb.Output"); } }

        private T Get<T>(string key)
        {
            object obj;
            if (!_environment.TryGetValue(key, out obj) || !(obj is T))
                return default(T);

            return (T)obj;
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