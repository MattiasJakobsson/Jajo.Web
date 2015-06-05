using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class DefaultOutputRenderer : IRenderToOutput
    {
        public virtual async Task Render(IDictionary<string, object> environment)
        {
            var renderers = environment.Get(OutputEnvironmentExtensions.OutputConstants.Renderers, new OutputSettings());

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