using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class RenderStreamOutput : IRenderOutput
    {
        public async Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput() as Stream;

            if (output == null)
                return null;

            output.Position = 0;
            var content = await new StreamReader(output).ReadToEndAsync();

            return new OutputRenderingResult(content, environment.GetRequest().Headers.Accept.Split(',').Select(x => x.Trim()).FirstOrDefault());
        }
    }
}