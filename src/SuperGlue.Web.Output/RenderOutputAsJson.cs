using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperGlue.Web.Output
{
    public class RenderOutputAsJson : IRenderOutput
    {
        public Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput();

            if (output == null)
                return null;

            var serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output));

            var stream = (Stream)new MemoryStream(serialized);

            stream.Position = 0;

            return Task.FromResult(new OutputRenderingResult(stream, "application/json"));
        }
    }
}