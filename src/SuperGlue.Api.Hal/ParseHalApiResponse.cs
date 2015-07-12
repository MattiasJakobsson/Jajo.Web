using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperGlue.ApiDiscovery;
using SuperGlue.HttpClient;

namespace SuperGlue.Api.Hal
{
    public class ParseHalApiResponse : IParseApiResponse
    {
        public string ContentType
        {
            get { return "application/hal+json"; }
        }

        public IApiResource Parse(IHttpResponse response)
        {
            var obj = JObject.Parse(response.RawBody);
            var resource = ParseRootResourceObject(obj);

            return resource;
        }

        private static ApiResource ParseRootResourceObject(JObject outer)
        {
            var links = new List<IApiLink>();
            var embedded = new List<IApiResource>();
            var state = new List<StateObject>();
            var forms = new List<IApiForm>();

            ParseResourceObject(outer, links, forms, embedded, state);

            return new ApiResource("root", state, forms, embedded, links);
        }

        private static ApiResource ParseEmbeddedResourceObject(JObject outer, string rel)
        {
            var links = new List<IApiLink>();
            var embedded = new List<IApiResource>();
            var state = new List<StateObject>();
            var forms = new List<IApiForm>();

            ParseResourceObject(outer, links, forms, embedded, state);

            return new ApiResource(rel, state, forms, embedded, links);
        }

        private static ApiForm ParseFormsResourceObject(JObject outer, string name)
        {
            var form = new ApiForm { Name = name };

            foreach (var inner in outer.Properties())
            {
                var value = inner.Value.ToString();

                if (string.IsNullOrEmpty(value))
                    continue;

                switch (inner.Name.ToLowerInvariant())
                {
                    case "action":
                        form.Action = TryCreateUri(value, UriKind.RelativeOrAbsolute);
                        break;
                    case "templated":
                        form.Templated = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "method":
                        form.Method = value;
                        break;
                    case "type":
                        form.Type = value;
                        break;
                    case "title":
                        form.Title = value;
                        break;
                    case "headers":
                        form.Headers = inner.Value.ToObject<IReadOnlyDictionary<string, string>>();
                        break;
                    case "schema":
                        form.Schema = inner.Value.ToObject<IReadOnlyDictionary<string, object>>();
                        break;
                }
            }

            return form;
        }

        private static void ParseResourceObject(JObject outer, List<IApiLink> links, List<IApiForm> forms, List<IApiResource> embedded, List<StateObject> state)
        {
            foreach (var inner in outer.Properties())
            {
                var type = inner.Value.Type.ToString();

                if (inner.Value.Type == JTokenType.Object)
                {
                    var value = (JObject)inner.Value;

                    switch (inner.Name)
                    {
                        case "_links":
                            links.AddRange(ParseObjectOrArrayOfObjects(value, ParseLinkObject));
                            break;
                        case "_embedded":
                            embedded.AddRange(ParseObjectOrArrayOfObjects(value, ParseEmbeddedResourceObject));
                            break;
                        case "_forms":
                            forms.AddRange(ParseObjectOrArrayOfObjects(value, ParseFormsResourceObject));
                            break;
                        default:
                            state.Add(new StateObject(inner.Name, value.ToString(), type));
                            break;
                    }
                }
                else
                {
                    var value = inner.Value.ToString();

                    switch (inner.Name)
                    {
                        case "_links":
                            throw new FormatException("Invalid value for _links: " + value);
                        case "_embedded":
                            throw new FormatException("Invalid value for _embedded: " + value);
                        case "_forms":
                            throw new FormatException("Invalid value for _forms: " + value);
                        default:
                            state.Add(new StateObject(inner.Name, value, type));
                            break;
                    }
                }
            }
        }

        private static ApiLink ParseLinkObject(JObject outer, string rel)
        {
            var link = new ApiLink { Rel = rel };

            foreach (var inner in outer.Properties())
            {
                var value = inner.Value.ToString();

                if (string.IsNullOrEmpty(value))
                    continue;

                switch (inner.Name.ToLowerInvariant())
                {
                    case "href":
                        link.Href = TryCreateUri(value, UriKind.RelativeOrAbsolute);
                        break;
                    case "templated":
                        link.Templated = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "type":
                        link.Type = value;
                        break;
                    case "deprication":
                        link.Deprecation = TryCreateUri(value, UriKind.Absolute);
                        break;
                    case "profile":
                        link.Profile = TryCreateUri(value, UriKind.Absolute);
                        break;
                    case "title":
                        link.Title = value;
                        break;
                    case "hreflang":
                        link.HrefLang = value;
                        break;
                }
            }

            return link;
        }

        private static IEnumerable<T> ParseObjectOrArrayOfObjects<T>(JObject outer, Func<JObject, string, T> factory)
        {
            foreach (var inner in outer.Properties())
            {
                var rel = inner.Name;

                if (inner.Value.Type == JTokenType.Array)
                    foreach (var child in inner.Value.Children<JObject>())
                        yield return factory(child, rel);
                else
                    yield return factory((JObject)inner.Value, rel);
            }
        }

        private static Uri TryCreateUri(string value, UriKind kind)
        {
            try
            {
                return new Uri(value, kind);
            }
            catch (UriFormatException)
            {
                return null;
            }
        }
    }
}