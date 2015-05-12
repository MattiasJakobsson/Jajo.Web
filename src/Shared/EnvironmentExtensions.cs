using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Jajo.Web
{
    internal static class EnvironmentExtensions
    {
        public static RequestData GetRequestData(this IDictionary<string, object> environment)
        {
            return new RequestData(environment.Get<string>("owin.RequestMethod"));
        }

        public static HeaderData GetHeaders(this IDictionary<string, object> environment)
        {
            return new HeaderData(new ReadOnlyDictionary<string, string[]>(environment.Get<IDictionary<string, string[]>>("owin.RequestHeaders")));
        }

        public static IDictionary<string, string[]> GetResponseHeaders(this IDictionary<string, object> environment)
        {
            return environment.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        public static Uri GetUri(this IDictionary<string, object> environment)
        {
            return new Uri(environment.Get<string>("owin.RequestScheme") + Uri.SchemeDelimiter + environment.GetHeaders().Host + environment.Get<string>("owin.RequestPathBase") + environment.Get<string>("owin.RequestPath") + environment.Get<string>("owin.RequestQueryString"));
        }

        public static async Task WriteToOutput(this IDictionary<string, object> environment, Stream data)
        {
            if (data == null)
                return;

            data.Position = 0;

            await data.CopyToAsync(environment.Get<Stream>("owin.ResponseBody"));
        }

        public static async Task WriteToOutput(this IDictionary<string, object> environment, string content)
        {
            var textWriter = new StreamWriter(environment.Get<Stream>("owin.ResponseBody"));

            await textWriter.WriteAsync(content);
            await textWriter.FlushAsync();
        }

        public static T Get<T>(this IDictionary<string, object> environment, string key)
        {
            object obj;
            if (!environment.TryGetValue(key, out obj) || !(obj is T))
                return default(T);

            return (T)obj;
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
            public string Accept { get { return GetHeader("Accept"); } }
            public string Host { get { return GetHeader("Host"); } }

            public string GetHeader(string key)
            {
                var headerUnmodified = GetHeaderUnmodified(RawHeaders, key);
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