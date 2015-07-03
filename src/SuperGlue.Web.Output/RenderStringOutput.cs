using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            var data = Encoding.UTF8.GetBytes(output);

            var stream = (Stream)new MemoryStream(data);

            stream.Position = 0;

            return Task.FromResult(new OutputRenderingResult(stream, environment.GetRequest().Headers.Accept.Split(',').Select(x => x.Trim()).FirstOrDefault()));
        }
    }
}