using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.ApiDiscovery;
using SuperGlue.HttpClient;

namespace SuperGlue.Api.Hal
{
    public class DefaultHalApiClient : IHandleApi
    {
        private readonly IHttpClient _httpClient;
        private readonly IHalJsonParser _halJsonParser;

        public DefaultHalApiClient(IHttpClient httpClient, IHalJsonParser halJsonParser)
        {
            _httpClient = httpClient;
            _halJsonParser = halJsonParser;
        }

        public bool Accepts(ApiDefinition definition)
        {
            return definition.Accepts.Contains("application/hal+json");
        }

        public async Task<IApiResource> Travel(ApiDefinition api, IDictionary<string, object> data, params IApiLinkTravelInstruction[] instructions)
        {
            var currentResource = await GetAsync(api.Location, null, api);

            foreach (var instruction in instructions)
            {
                var documentLink = instruction.TravelTo(currentResource);

                var uri = documentLink.Href;

                if (documentLink.Templated)
                {
                    var template = new UriTemplate(uri.ToString());

                    foreach (var parameterName in template.GetParameterNames())
                    {
                        if (data.ContainsKey(parameterName))
                            template.SetParameter(parameterName, data[parameterName]);
                        else if (currentResource.State.ContainsKey(parameterName))
                            template.SetParameter(parameterName, currentResource.State[parameterName]);
                    }

                    uri = new Uri(template.Resolve());
                }

                currentResource = await GetAsync(uri, documentLink.Type, api);
            }

            return currentResource;
        }

        public async Task<string> ExecuteForm(IApiResource resource, IDictionary<string, object> data, IFormTravelInstruction travelInstruction)
        {
            var form = travelInstruction.TravelTo(resource);

            var formData = new Dictionary<string, object>();

            foreach (var item in form.Schema)
            {
                if (data.ContainsKey(item.Key))
                    formData[item.Key] = item.Value;
                else if (resource.State.ContainsKey(item.Key))
                    formData[item.Key] = resource.State[item.Key];
                else
                    formData[item.Key] = item.Value;
            }

            var uri = form.Action;

            if (form.Templated)
            {
                var template = new UriTemplate(uri.ToString());

                foreach (var parameterName in template.GetParameterNames())
                {
                    if (data.ContainsKey(parameterName))
                        template.SetParameter(parameterName, data[parameterName]);
                    else if (resource.State.ContainsKey(parameterName))
                        template.SetParameter(parameterName, resource.State[parameterName]);
                }

                uri = new Uri(template.Resolve());
            }

            var request = _httpClient
                .Start(uri.ToString())
                .Method(form.Method);

            request = formData
                .Aggregate(request, (current, item) => current.Parameter(item.Key, item.Value));

            request = form
                .Headers
                .Aggregate(request, (current, header) => current.Header(header.Key, header.Value));

            return (await request.Send()).RawBody;
        }

        private async Task<IApiResource> GetAsync(Uri uri, string type, ApiDefinition definition)
        {
            var response = await _httpClient
                .Start(uri.ToString())
                .ContentType(type ?? "application/hal+json")
                .Send();

            return _halJsonParser.ParseResource(definition, response.RawBody);
        }
    }
}