using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output
{
    public class DefaultOutputRenderer : IRenderToOutput
    {
        public virtual async Task Render(IDictionary<string, object> environment)
        {
            var renderers = environment.GetSettings<OutputSettings>();

            var renderer = renderers.FindRenderer(environment);

            if (renderer == null)
                return;

            var result = await renderer.Render(environment).ConfigureAwait(false);

            environment.SetOutputResult(result);
        }
    }
}