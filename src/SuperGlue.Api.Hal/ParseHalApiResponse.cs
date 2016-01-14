using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SuperGlue.ApiDiscovery;
using SuperGlue.HttpClient;

namespace SuperGlue.Api.Hal
{
    public class ParseHalApiResponse : IParseApiResponse
    {
        public string ContentType => "application/hal+json";

        public async Task<ApiResource> Parse(IHttpResponse response, ApiDefinition definition)
        {
            var obj = JObject.Parse(await response.ReadRawBody().ConfigureAwait(false));
            var resource = ParseRootResourceObject(obj, definition);

            return resource;
        }

        private static ApiResource ParseRootResourceObject(JObject outer, ApiDefinition definition)
        {
            var links = new List<ApiLink>();
            var embedded = new List<ApiResource>();
            var forms = new List<ApiForm>();

            ParseResourceObject(outer, links, forms, embedded, definition);

            return new ApiResource("root", forms, embedded, links, definition, outer.ToObject);
        }

        private static ApiResource ParseEmbeddedResourceObject(JObject outer, string rel, ApiDefinition definition)
        {
            var links = new List<ApiLink>();
            var embedded = new List<ApiResource>();
            var forms = new List<ApiForm>();

            ParseResourceObject(outer, links, forms, embedded, definition);

            return new ApiResource(rel, forms, embedded, links, definition, outer.ToObject);
        }

        private static ApiForm ParseFormsResourceObject(JObject outer, string name, ApiDefinition definition)
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

        private static void ParseResourceObject(JObject outer, List<ApiLink> links, List<ApiForm> forms, List<ApiResource> embedded, ApiDefinition definition)
        {
            foreach (var inner in outer.Properties())
            {
                if (inner.Value.Type == JTokenType.Object)
                {
                    var value = (JObject)inner.Value;

                    switch (inner.Name)
                    {
                        case "_links":
                            links.AddRange(ParseObjectOrArrayOfObjects(value, definition, ParseLinkObject));
                            break;
                        case "_embedded":
                            embedded.AddRange(ParseObjectOrArrayOfObjects(value, definition, ParseEmbeddedResourceObject));
                            break;
                        case "_forms":
                            forms.AddRange(ParseObjectOrArrayOfObjects(value, definition, ParseFormsResourceObject));
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
                    }
                }
            }
        }

        private static ApiLink ParseLinkObject(JObject outer, string rel, ApiDefinition definition)
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

        private static IEnumerable<T> ParseObjectOrArrayOfObjects<T>(JObject outer, ApiDefinition definition, Func<JObject, string, ApiDefinition, T> factory)
        {
            foreach (var inner in outer.Properties())
            {
                var rel = inner.Name;

                if (inner.Value.Type == JTokenType.Array)
                    foreach (var child in inner.Value.Children<JObject>())
                        yield return factory(child, rel, definition);
                else
                    yield return factory((JObject)inner.Value, rel, definition);
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