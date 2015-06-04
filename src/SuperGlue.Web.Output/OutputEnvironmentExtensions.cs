using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public static class OutputEnvironmentExtensions
    {
        public static class OutputConstants
        {
            public const string Renderers = "superglue.OutputRenderers";
        }

        public static async Task Render(this IDictionary<string, object> environment)
        {
            var renderers = environment.Get(OutputConstants.Renderers, new OutputSettings());

            var renderer = renderers.FindRenderer(environment);

            if (renderer == null)
                return;

            var result = await renderer.Render(environment);

            if (result == null)
                return;

            var response = environment.GetResponse();

            response.Headers.ContentType = result.ContentType;

            await response.Write(result.Body);
        }
    }
}