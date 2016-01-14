using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public class DefaultApiRequestExecutor : IExecuteApiRequests
    {
        private readonly IEnumerable<IParseApiResponse> _responseParsers;
        private readonly IHttpClient _httpClient;
        private readonly IDictionary<string, object> _environment;

        public DefaultApiRequestExecutor(IEnumerable<IParseApiResponse> responseParsers, IHttpClient httpClient, IDictionary<string, object> environment)
        {
            _responseParsers = responseParsers;
            _httpClient = httpClient;
            _environment = environment;
        }

        public async Task<ApiResource> ExecuteQuery(ApiDefinition definition, IEnumerable<IApiLinkTravelInstruction> instructions, IDictionary<string, object> data)
        {
            var parser = _responseParsers.FirstOrDefault(x => definition.Accepts.Contains(x.ContentType));

            if(parser == null)
                throw new ApiException($"Can't handle api named: {definition.Name}");

            var currentResource = await GetAsync(definition.Location, parser, definition).ConfigureAwait(false);

            if (currentResource == null)
            {
                _environment.Log("Can't find resource for url: {0}.", LogLevel.Error, definition.Location);

                return null;
            }

            foreach (var instruction in instructions)
            {
                var documentLink = instruction.TravelTo(currentResource);

                var uri = documentLink.Href;

                if (documentLink.Templated)
                {
                    var template = new UriTemplate(uri.ToString());

                    var state = currentResource.StateAs<IDictionary<string, object>>();

                    foreach (var parameterName in template.GetParameterNames())
                    {
                        if (data.ContainsKey(parameterName))
                            template.SetParameter(parameterName, data[parameterName]);
                        else if (state.ContainsKey(parameterName))
                            template.SetParameter(parameterName, state[parameterName]);
                    }

                    uri = new Uri(template.Resolve(), UriKind.RelativeOrAbsolute);
                }

                if (!uri.IsAbsoluteUri)
                    uri = new Uri(definition.Location.Scheme + Uri.SchemeDelimiter + definition.Location.Host + (definition.Location.IsDefaultPort ? "" : string.Concat(":", definition.Location.Port)) + uri);

                currentResource = await GetAsync(uri, parser, definition).ConfigureAwait(false);

                if (currentResource == null)
                {
                    _environment.Log("Can't find resource for url: {0}.", LogLevel.Error, uri);

                    return null;
                }
            }

            return currentResource;
        }

        public async Task<IHttpResponse> ExecuteForm(ApiResource resource, IFormTravelInstruction travelInstruction, IDictionary<string, object> data)
        {
            var form = travelInstruction.TravelTo(resource);
            var definition = resource.Definition;

            var formData = new Dictionary<string, object>();

            var state = resource.StateAs<IDictionary<string, object>>();

            foreach (var item in form.Schema)
            {
                if (data.ContainsKey(item.Key))
                    formData[item.Key] = data[item.Key];
                else if (state.ContainsKey(item.Key))
                    formData[item.Key] = state[item.Key];
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
                    else if (state.ContainsKey(parameterName))
                        template.SetParameter(parameterName, state[parameterName]);
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

            if (!string.IsNullOrEmpty(form.Type))
                request = request.ContentType(form.Type);

            request = formData
                .Aggregate(request, (current, item) => current.Parameter(item.Key, item.Value));

            var response = await request.Send().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                _environment.Log("Execution of form at: {0} returned status code: {1}.", LogLevel.Error, uri, response.StatusCode);

            return response;
        }

        private async Task<ApiResource> GetAsync(Uri uri, IParseApiResponse parser, ApiDefinition definition)
        {
            var response = await _httpClient
                .Start(uri)
                .ModifyHeaders(x => x.Accept.Add(new MediaTypeWithQualityHeaderValue(parser.ContentType)))
                .Send().ConfigureAwait(false);

            _environment.Log("Api request to url: {0} finished with status code: {1}.", LogLevel.Debug, uri, response.StatusCode);

            if (!response.IsSuccessStatusCode)
                return null;

            return await parser.Parse(response, definition).ConfigureAwait(false);
        }
    }
}