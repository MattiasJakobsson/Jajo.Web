using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web
{
    internal static class WebEnvironmentExtensions
    {
        public static WebRequest GetRequest(this IDictionary<string, object> environment)
        {
            return new WebRequest(environment);
        }

        public static WebResponse GetResponse(this IDictionary<string, object> environment)
        {
            return new WebResponse(environment);
        }

        internal static class OwinConstants
        {
            // http://owin.org/spec/owin-1.0.0.html

            public const string RequestScheme = "owin.RequestScheme";
            public const string RequestMethod = "owin.RequestMethod";
            public const string RequestPathBase = "owin.RequestPathBase";
            public const string RequestPath = "owin.RequestPath";
            public const string RequestQueryString = "owin.RequestQueryString";
            public const string RequestProtocol = "owin.RequestProtocol";
            public const string RequestHeaders = "owin.RequestHeaders";
            public const string RequestBody = "owin.RequestBody";

            // http://owin.org/spec/owin-1.0.0.html

            public const string ResponseStatusCode = "owin.ResponseStatusCode";
            public const string ResponseReasonPhrase = "owin.ResponseReasonPhrase";
            public const string ResponseProtocol = "owin.ResponseProtocol";
            public const string ResponseHeaders = "owin.ResponseHeade";
            public const string ResponseBody = "owin.ResponseBody";

            internal static class CommonKeys
            {
                public const string ClientCertificate = "ssl.ClientCertificate";
                public const string RemoteIpAddress = "server.RemoteIpAddress";
                public const string RemotePort = "server.RemotePort";
                public const string LocalIpAddress = "server.LocalIpAddress";
                public const string LocalPort = "server.LocalPort";
                public const string IsLocal = "server.IsLocal";
                public const string TraceOutput = "host.TraceOutput";
                public const string Addresses = "host.Addresses";
                public const string AppName = "host.AppName";
                public const string Capabilities = "server.Capabilities";
                public const string OnSendingHeaders = "server.OnSendingHeaders";
                public const string OnAppDisposing = "host.OnAppDisposing";
                public const string Scheme = "scheme";
                public const string Host = "host";
                public const string Port = "port";
                public const string Path = "path";
            }
        }

        internal static class HeadersConstants
        {
            internal const string ContentType = "Content-Type";
            internal const string CacheControl = "Cache-Control";
            internal const string MediaType = "Media-Type";
            internal const string Accept = "Accept";
            internal const string Host = "Host";
            internal const string ETag = "ETag";
            internal const string Location = "Location";
            internal const string ContentLength = "Content-Length";
            internal const string SetCookie = "Set-Cookie";
            internal const string Expires = "Expires";
        }

        internal class WebRequest
        {
            private readonly IDictionary<string, object> _environment;

            public WebRequest(IDictionary<string, object> environment)
            {
                _environment = environment;
            }

            public string Method { get { return _environment.Get<string>(OwinConstants.RequestMethod); } }
            public string Scheme { get { return _environment.Get<string>(OwinConstants.RequestScheme); } }
            public bool IsSecure { get { return string.Equals(Scheme, "HTTPS", StringComparison.OrdinalIgnoreCase); } }

            public string Host
            {
                get
                {
                    var host = Headers.GetHeader("Host");
                    if (!string.IsNullOrWhiteSpace(host))
                    {
                        return host;
                    }

                    var localIpAddress = LocalIpAddress ?? "localhost";
                    var localPort = _environment.Get<string>(OwinConstants.CommonKeys.LocalPort);
                    return string.IsNullOrWhiteSpace(localPort) ? localIpAddress : (localIpAddress + ":" + localPort);
                }
            }

            public string PathBase { get { return _environment.Get<string>(OwinConstants.RequestPathBase); } }
            public string Path { get { return _environment.Get<string>(OwinConstants.RequestPath); } }
            public string QueryString { get { return _environment.Get<string>(OwinConstants.RequestQueryString); } }
            public Uri Uri { get { return new Uri(Scheme + Uri.SchemeDelimiter + Host + PathBase + Path + QueryString); } }
            public string Protocol { get { return _environment.Get<string>(OwinConstants.RequestProtocol); } }
            public RequestHeaders Headers { get { return new RequestHeaders(new ReadOnlyDictionary<string, string[]>(_environment.Get<IDictionary<string, string[]>>(OwinConstants.RequestHeaders))); } }
            public RequestCookieCollection Cookies { get { return new RequestCookieCollection(GetCookies()); } }
            public Stream Body { get { return _environment.Get<Stream>(OwinConstants.RequestBody); } }
            public string LocalIpAddress { get { return _environment.Get<string>(OwinConstants.CommonKeys.LocalIpAddress); } }

            public int? LocalPort
            {
                get
                {
                    int value;
                    if (int.TryParse(_environment.Get<string>(OwinConstants.CommonKeys.LocalPort), out value))
                        return value;

                    return null;
                }
            }

            public string RemoteIpAddress { get { return _environment.Get<string>(OwinConstants.CommonKeys.RemoteIpAddress); } }

            public int? RemotePort
            {
                get
                {
                    int value;
                    if (int.TryParse(_environment.Get<string>(OwinConstants.CommonKeys.RemotePort), out value))
                        return value;

                    return null;
                }
            }

            private static readonly char[] SemicolonAndComma = { ';', ',' };

            private IDictionary<string, string> GetCookies()
            {
                var cookies = new Dictionary<string, string>();

                var text = Headers.GetHeader("Cookie");

                ParseDelimited(text, SemicolonAndComma, AddCookieCallback, cookies);

                return cookies;
            }

            private static readonly Action<string, string, object> AddCookieCallback = (name, value, state) =>
            {
                var dictionary = (IDictionary<string, string>)state;
                if (!dictionary.ContainsKey(name))
                {
                    dictionary.Add(name, value);
                }
            };

            private static void ParseDelimited(string text, char[] delimiters, Action<string, string, object> callback, object state)
            {
                var textLength = text.Length;
                var equalIndex = text.IndexOf('=');
                if (equalIndex == -1)
                {
                    equalIndex = textLength;
                }
                var scanIndex = 0;
                while (scanIndex < textLength)
                {
                    var delimiterIndex = text.IndexOfAny(delimiters, scanIndex);
                    if (delimiterIndex == -1)
                    {
                        delimiterIndex = textLength;
                    }
                    if (equalIndex < delimiterIndex)
                    {
                        while (scanIndex != equalIndex && char.IsWhiteSpace(text[scanIndex]))
                        {
                            ++scanIndex;
                        }
                        var name = text.Substring(scanIndex, equalIndex - scanIndex);
                        var value = text.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);
                        callback(
                            Uri.UnescapeDataString(name.Replace('+', ' ')),
                            Uri.UnescapeDataString(value.Replace('+', ' ')),
                            state);
                        equalIndex = text.IndexOf('=', delimiterIndex);

                        if (equalIndex == -1)
                        {
                            equalIndex = textLength;
                        }
                    }
                    scanIndex = delimiterIndex + 1;
                }
            }

            public class RequestHeaders
            {
                public RequestHeaders(IReadOnlyDictionary<string, string[]> rawHeaders)
                {
                    RawHeaders = rawHeaders;
                }

                public IReadOnlyDictionary<string, string[]> RawHeaders { get; private set; }
                public string ContentType { get { return GetHeader(HeadersConstants.ContentType); } }
                public string MediaType { get { return GetHeader(HeadersConstants.MediaType); } }
                public string Accept { get { return GetHeader(HeadersConstants.Accept); } }

                public string GetHeader(string key)
                {
                    var values = GetHeaderUnmodified(key);
                    return values == null ? null : string.Join(",", values);
                }

                private string[] GetHeaderUnmodified(string key)
                {
                    if (RawHeaders == null)
                        throw new ArgumentNullException("headers");

                    string[] values;
                    return RawHeaders.TryGetValue(key, out values) ? values : null;
                }
            }

            public class RequestCookieCollection : IEnumerable<KeyValuePair<string, string>>
            {
                public RequestCookieCollection(IDictionary<string, string> store)
                {
                    if (store == null)
                        throw new ArgumentNullException("store");

                    Store = store;
                }

                private IDictionary<string, string> Store { get; set; }

                public string this[string key]
                {
                    get
                    {
                        string value;
                        Store.TryGetValue(key, out value);
                        return value;
                    }
                }

                public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
                {
                    return Store.GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
        }

        internal class WebResponse
        {
            private readonly IDictionary<string, object> _environment;

            public WebResponse(IDictionary<string, object> environment)
            {
                _environment = environment;
            }

            public int StatusCode
            {
                get { return _environment.Get(OwinConstants.ResponseStatusCode, 200); }
                set { Set(OwinConstants.ResponseStatusCode, value); }
            }

            public string ReasonPhrase
            {
                get { return _environment.Get<string>(OwinConstants.ResponseReasonPhrase); }
                set { Set(OwinConstants.ResponseReasonPhrase, value); }
            }

            public string Protocol
            {
                get { return _environment.Get<string>(OwinConstants.ResponseProtocol); }
                set { Set(OwinConstants.ResponseProtocol, value); }
            }

            public ResponseHeaders Headers { get { return new ResponseHeaders(_environment.Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeaders)); } }
            public ResponseCookieCollection Cookies { get { return new ResponseCookieCollection(Headers); } }
            public Stream Body { get { return _environment.Get<Stream>(OwinConstants.ResponseBody); } }

            public async Task Write(string text)
            {
                var textWriter = new StreamWriter(Body);

                await textWriter.WriteAsync(text);
                await textWriter.FlushAsync();
            }

            public async Task Write(Stream stream)
            {
                if (stream == null)
                    return;

                stream.Position = 0;

                await stream.CopyToAsync(Body);
            }

            private void Set(string key, object value)
            {
                _environment[key] = value;
            }

            public class ResponseHeaders
            {
                internal const string HttpDateFormat = "r";

                public ResponseHeaders(IDictionary<string, string[]> rawHeaders)
                {
                    RawHeaders = rawHeaders;
                }

                public IDictionary<string, string[]> RawHeaders { get; private set; }

                public virtual long? ContentLength
                {
                    get
                    {
                        long value;
                        if (long.TryParse(GetHeader(HeadersConstants.ContentLength), out value))
                            return value;

                        return null;
                    }
                    set
                    {
                        if (value.HasValue)
                            SetHeader(HeadersConstants.ContentLength, value.Value.ToString(CultureInfo.InvariantCulture));
                        else
                            RawHeaders.Remove(HeadersConstants.ContentLength);
                    }
                }

                public string ContentType
                {
                    get { return GetHeader(HeadersConstants.ContentType); }
                    set { SetHeader(HeadersConstants.ContentType, value); }
                }

                public string Location
                {
                    get { return GetHeader(HeadersConstants.Location); }
                    set { SetHeader(HeadersConstants.Location, value); }
                }

                public DateTimeOffset? Expires
                {
                    get
                    {
                        DateTimeOffset value;
                        if (DateTimeOffset.TryParse(GetHeader(HeadersConstants.Expires),
                            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out value))
                        {
                            return value;
                        }
                        return null;
                    }
                    set
                    {
                        if (value.HasValue)
                            SetHeader(HeadersConstants.Expires, value.Value.ToString(HttpDateFormat, CultureInfo.InvariantCulture));
                        else
                            RawHeaders.Remove(HeadersConstants.Expires);
                    }
                }
                public string ETag
                {
                    get { return GetHeader(HeadersConstants.ETag); }
                    set { SetHeader(HeadersConstants.ETag, value); }
                }

                public string GetHeader(string key)
                {
                    var values = GetHeaderUnmodified(key);
                    return values == null ? null : string.Join(",", values);
                }

                public void SetHeader(string key, string value)
                {
                    if (string.IsNullOrWhiteSpace(key))
                        throw new ArgumentNullException("key");

                    if (string.IsNullOrWhiteSpace(value))
                        RawHeaders.Remove(key);
                    else
                        RawHeaders[key] = new[] { value };
                }

                public IList<string> GetValues(string key)
                {
                    return GetHeaderUnmodified(key);
                }

                public void SetValues(string key, params string[] values)
                {
                    SetHeaderUnmodified(key, values);
                }

                public void AppendValues(string key, params string[] values)
                {
                    if (values == null || values.Length == 0)
                        return;

                    var existing = GetHeaderUnmodified(key);
                    SetHeaderUnmodified(key, existing == null ? values : existing.Concat(values));
                }

                private string[] GetHeaderUnmodified(string key)
                {
                    string[] values;
                    return RawHeaders.TryGetValue(key, out values) ? values : null;
                }

                private void SetHeaderUnmodified(string key, IEnumerable<string> values)
                {
                    RawHeaders[key] = values.ToArray();
                }
            }

            public class ResponseCookieCollection
            {
                private readonly ResponseHeaders _headers;

                public ResponseCookieCollection(ResponseHeaders headers)
                {
                    if (headers == null)
                        throw new ArgumentNullException("headers");

                    _headers = headers;
                }

                public void Append(string key, string value)
                {
                    _headers.AppendValues(HeadersConstants.SetCookie, Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value) + "; path=/");
                }

                public void Append(string key, string value, CookieOptions options)
                {
                    if (options == null)
                        throw new ArgumentNullException("options");

                    var domainHasValue = !string.IsNullOrEmpty(options.Domain);
                    var pathHasValue = !string.IsNullOrEmpty(options.Path);
                    var expiresHasValue = options.Expires.HasValue;

                    var setCookieValue = string.Concat(
                        Uri.EscapeDataString(key),
                        "=",
                        Uri.EscapeDataString(value ?? string.Empty),
                        !domainHasValue ? null : "; domain=",
                        !domainHasValue ? null : options.Domain,
                        !pathHasValue ? null : "; path=",
                        !pathHasValue ? null : options.Path,
                        !expiresHasValue ? null : "; expires=",
                        !expiresHasValue ? null : options.Expires.Value.ToString("ddd, dd-MMM-yyyy HH:mm:ss ", CultureInfo.InvariantCulture) + "GMT",
                        !options.Secure ? null : "; secure",
                        !options.HttpOnly ? null : "; HttpOnly");

                    _headers.AppendValues("Set-Cookie", setCookieValue);
                }

                public void Delete(string key)
                {
                    Func<string, bool> predicate = value => value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase);

                    var deleteCookies = new[] { Uri.EscapeDataString(key) + "=; expires=Thu, 01-Jan-1970 00:00:00 GMT" };

                    IList<string> existingValues = _headers.GetValues(HeadersConstants.SetCookie);
                    if (existingValues == null || existingValues.Count == 0)
                    {
                        _headers.SetValues(HeadersConstants.SetCookie, deleteCookies);
                    }
                    else
                    {
                        _headers.SetValues(HeadersConstants.SetCookie, existingValues.Where(value => !predicate(value)).Concat(deleteCookies).ToArray());
                    }
                }

                public void Delete(string key, CookieOptions options)
                {
                    if (options == null)
                        throw new ArgumentNullException("options");

                    var domainHasValue = !string.IsNullOrEmpty(options.Domain);
                    var pathHasValue = !string.IsNullOrEmpty(options.Path);

                    Func<string, bool> rejectPredicate;
                    if (domainHasValue)
                        rejectPredicate = value => value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase) && value.IndexOf("domain=" + options.Domain, StringComparison.OrdinalIgnoreCase) != -1;
                    else if (pathHasValue)
                        rejectPredicate = value => value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase) && value.IndexOf("path=" + options.Path, StringComparison.OrdinalIgnoreCase) != -1;
                    else
                        rejectPredicate = value => value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase);

                    var existingValues = _headers.GetValues(HeadersConstants.SetCookie);
                    if (existingValues != null)
                        _headers.SetValues(HeadersConstants.SetCookie, existingValues.Where(value => !rejectPredicate(value)).ToArray());

                    Append(key, string.Empty, new CookieOptions
                    {
                        Path = options.Path,
                        Domain = options.Domain,
                        Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    });
                }
            }
        }

        public class CookieOptions
        {
            public CookieOptions()
            {
                Path = "/";
            }

            public string Domain { get; set; }
            public string Path { get; set; }
            public DateTime? Expires { get; set; }
            public bool Secure { get; set; }
            public bool HttpOnly { get; set; }
        }
    }
}
