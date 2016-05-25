using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class RenderStreamOutput : IRenderOutput
    {
        public Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput() as Stream;

            if (output == null)
                return Task.FromResult<OutputRenderingResult>(null);

            return Task.FromResult(new OutputRenderingResult(output, environment.GetRequest().Headers.Accept.Split(',').Select(x => x.Trim()).FirstOrDefault()));
        }
    }
}