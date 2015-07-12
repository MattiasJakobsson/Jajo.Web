using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperGlue.ApiDiscovery;

namespace SuperGlue.Api.Hal
{
    public class HalJsonParser : IHalJsonParser
    {
        public IApiResource ParseResource(ApiDefinition api, string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException("json");

            var obj = JObject.Parse(json);
            var resource = ParseRootResourceObject(api, obj);

            return resource;
        }

        private static ApiResource ParseRootResourceObject(ApiDefinition api, JObject outer)
        {
            var links = new List<IApiLink>();
            var embedded = new List<IApiResource>();
            var state = new List<StateObject>();
            var forms = new List<IApiForm>();

            ParseResourceObject(outer, api, links, forms, embedded, state);

            return new ApiResource(api, "root", state, forms, embedded, links);
        }

        private static ApiResource ParseEmbeddedResourceObject(JObject outer, string rel, ApiDefinition api)
        {
            var links = new List<IApiLink>();
            var embedded = new List<IApiResource>();
            var state = new List<StateObject>();
            var forms = new List<IApiForm>();

            ParseResourceObject(outer, api, links, forms, embedded, state);

            return new ApiResource(api, rel, state, forms, embedded, links);
        }

        private static ApiForm ParseFormsResourceObject(JObject outer, string name, ApiDefinition api)
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

        private static void ParseResourceObject(JObject outer, ApiDefinition api, List<IApiLink> links, List<IApiForm> forms, List<IApiResource> embedded, List<StateObject> state)
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
                            links.AddRange(ParseObjectOrArrayOfObjects(value, api, ParseLinkObject));
                            break;
                        case "_embedded":
                            embedded.AddRange(ParseObjectOrArrayOfObjects(value, api, ParseEmbeddedResourceObject));
                            break;
                        case "_forms":
                            forms.AddRange(ParseObjectOrArrayOfObjects(value, api, ParseFormsResourceObject));
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

        private static ApiLink ParseLinkObject(JObject outer, string rel, ApiDefinition api)
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