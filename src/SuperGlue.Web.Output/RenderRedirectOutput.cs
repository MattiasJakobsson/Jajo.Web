using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class RenderRedirectOutput : IRenderOutput
    {
        public Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            return Task.Factory.StartNew(() =>
            {
                var output = environment.GetOutput() as IRedirectable;

                if (output == null)
                    return new OutputRenderingResult(new MemoryStream(), "");

                var redirectTo = output.GetUrl(environment);

                environment.GetResponse().StatusCode = 301;
                environment.GetResponse().Headers.Location = redirectTo;

                return new OutputRenderingResult(new MemoryStream(), "");
            });
        }
    }
}