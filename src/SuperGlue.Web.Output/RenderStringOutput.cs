using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class RenderStringOutput : IRenderOutput
    {
        public Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput() as string;

            if (output == null)
                return null;

            return Task.FromResult(new OutputRenderingResult(output, environment.GetRequest().Headers.Accept.Split(',').Select(x => x.Trim()).FirstOrDefault()));
        }
    }
}