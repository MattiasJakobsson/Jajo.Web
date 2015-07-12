using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.HttpClient;

namespace SuperGlue.ApiDiscovery
{
    public class DefaultApiClient : IApiClient
    {
        private readonly IEnumerable<IParseApiResponse> _responseParsers;
        private readonly IHttpClient _httpClient;

        public DefaultApiClient(IEnumerable<IParseApiResponse> responseParsers, IHttpClient httpClient)
        {
            _responseParsers = responseParsers;
            _httpClient = httpClient;
        }

        public async Task<IApiResource> TravelTo(ApiDefinition api, IDictionary<string, object> data, params IApiLinkTravelInstruction[] instructions)
        {
            var parser = _responseParsers.FirstOrDefault(x => api.Accepts.Contains(x.ContentType));

            if(parser == null)
                throw new ApiException(string.Format("Can't handle api named: {0}", api.Name));

            var currentResource = await GetAsync(api.Location, parser);

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

                currentResource = await GetAsync(uri, parser);
            }

            return currentResource;
        }

        public Task<IHttpResponse> ExecuteForm(IApiResource resource, IDictionary<string, object> data, IFormTravelInstruction travelInstruction)
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

            return request.Send();
        }

        private async Task<IApiResource> GetAsync(Uri uri, IParseApiResponse parser)
        {
            var response = await _httpClient
                .Start(uri.ToString())
                .ContentType(parser.ContentType)
                .Send();

            return parser.Parse(response);
        }
    }
}