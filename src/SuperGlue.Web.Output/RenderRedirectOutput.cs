using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class RenderRedirectOutput : IRenderOutput
    {
        public Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput() as IRedirectable;

            if (output == null)
                return Task.FromResult(new OutputRenderingResult("", ""));

            var redirectTo = output.GetUrl(environment);

            environment.GetResponse().StatusCode = 301;
            environment.GetResponse().Headers.Location = redirectTo;

            return Task.FromResult(new OutputRenderingResult("", ""));
        }
    }
}