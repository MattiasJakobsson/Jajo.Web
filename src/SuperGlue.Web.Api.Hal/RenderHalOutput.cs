using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperGlue.ApiDiscovery;
using SuperGlue.Web.ApiDiscovery;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Api.Hal
{
    public class RenderHalOutput : IRenderApiOutput
    {
        public string Type
        {
            get { return "application/hal+json"; }
        }

        public Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput() as IApiResponse;

            if (output == null)
                return null;

            var response = BuildDocumentFor(output);

            var serialized = JsonConvert.SerializeObject(response);

            return Task.FromResult(new OutputRenderingResult(serialized, "application/hal+json"));
        }

        private static IDictionary<string, object> BuildDocumentFor(IApiResponse apiResponse)
        {
            var response = new Dictionary<string, object>();

            var links = apiResponse.GetLinks().ToList();
            var forms = apiResponse.GetForms().ToList();
            var embedded = new Dictionary<string, IEnumerable<IDictionary<string, object>>>();

            foreach (var child in apiResponse.GetChildren())
                embedded[child.Key] = child.Value.Select(BuildDocumentFor);

            response["_links"] = BuildLinks(links);

            if (forms.Any())
                response["_forms"] = BuildForms(forms);

            foreach (var item in BuildState(apiResponse))
                response[item.Key] = item.Value;

            if (embedded.Any())
                response["_embedded"] = embedded;

            return response;
        }

        private static IDictionary<string, object> BuildLinks(IEnumerable<ApiLink> apiLinks)
        {
            var links = new Dictionary<string, object>();

            foreach (var apiLink in apiLinks.GroupBy(x => x.Rel))
            {
                var currentLinks = apiLink.Select(x => new
                {
                    href = x.Href,
                    deprecation = x.Deprecation,
                    hrefLang = x.HrefLang,
                    profile = x.Profile,
                    templated = x.Templated,
                    title = x.Title,
                    type = x.Type
                }).ToList();

                if (currentLinks.Count == 1)
                    links[apiLink.Key] = currentLinks.Single();
                else
                    links[apiLink.Key] = currentLinks;
            }

            return links;
        }

        private static IDictionary<string, object> BuildForms(IEnumerable<ApiForm> apiForms)
        {
            var forms = new Dictionary<string, object>();

            foreach (var apiForm in apiForms)
            {
                forms[apiForm.Name] = new
                {
                    action = apiForm.Action,
                    headers = apiForm.Headers,
                    templated = apiForm.Templated,
                    method = apiForm.Method,
                    schema = apiForm.Schema,
                    title = apiForm.Title,
                    type = apiForm.Type
                };
            }

            return forms;
        }

        private static IDictionary<string, object> BuildState(object state)
        {
            var result = new Dictionary<string, object>();

            foreach (var property in state.GetType().GetProperties())
                result[property.Name] = property.GetValue(state);

            return result;
        }
    }
}