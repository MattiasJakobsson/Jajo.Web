using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public class DefaultApiRequestExecutor : IExecuteApiRequests
    {
        private readonly IEnumerable<IParseApiResponse> _responseParsers;
        private readonly IHttpClient _httpClient;

        public DefaultApiRequestExecutor(IEnumerable<IParseApiResponse> responseParsers, IHttpClient httpClient)
        {
            _responseParsers = responseParsers;
            _httpClient = httpClient;
        }

        public async Task<ApiResource> ExecuteQuery(ApiDefinition definition, IEnumerable<IApiLinkTravelInstruction> instructions, IDictionary<string, object> data)
        {
            var parser = _responseParsers.FirstOrDefault(x => definition.Accepts.Contains(x.ContentType));

            if(parser == null)
                throw new ApiException(string.Format("Can't handle api named: {0}", definition.Name));

            var currentResource = await GetAsync(definition.Location, parser, definition);

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

                    uri = new Uri(template.Resolve(), UriKind.RelativeOrAbsolute);
                }

                if (!uri.IsAbsoluteUri)
                    uri = new Uri(definition.Location.Scheme + Uri.SchemeDelimiter + definition.Location.Host + (definition.Location.IsDefaultPort ? "" : string.Concat(":", definition.Location.Port)) + uri);

                currentResource = await GetAsync(uri, parser, definition);
            }

            return currentResource;
        }

        public Task<IHttpResponse> ExecuteForm(ApiResource resource, IFormTravelInstruction travelInstruction, IDictionary<string, object> data)
        {
            var form = travelInstruction.TravelTo(resource);
            var definition = resource.Definition;

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

                uri = new Uri(template.Resolve(), UriKind.RelativeOrAbsolute);
            }

            if (!uri.IsAbsoluteUri)
                uri = new Uri(definition.Location.Scheme + Uri.SchemeDelimiter + definition.Location.Host + (definition.Location.IsDefaultPort ? "" : string.Concat(":", definition.Location.Port)) + uri);

            var request = _httpClient
                .Start(uri)
                .Method(form.Method)
                .ModifyHeaders(x =>
                {
                    foreach (var header in (form.Headers ?? new Dictionary<string, string>()))
                        x.Add(header.Key, header.Value);
                });

            request = formData
                .Aggregate(request, (current, item) => current.Parameter(item.Key, item.Value));

            return request.Send();
        }

        private async Task<ApiResource> GetAsync(Uri uri, IParseApiResponse parser, ApiDefinition definition)
        {
            var response = await _httpClient
                .Start(uri)
                .ContentType(parser.ContentType)
                .Send();

            return await parser.Parse(response, definition);
        }
    }
}