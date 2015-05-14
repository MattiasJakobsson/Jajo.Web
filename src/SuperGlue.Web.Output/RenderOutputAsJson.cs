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
            return Task.Factory.StartNew(() =>
            {
                var output = environment.GetOutput();

                if (output == null)
                    return null;

                var serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output));

                var stream = (Stream)new MemoryStream(serialized);

                stream.Position = 0;

                return new OutputRenderingResult(stream, "application/json");
            });
        }
    }
}