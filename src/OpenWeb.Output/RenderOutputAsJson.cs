using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenWeb.Output
{
    public class RenderOutputAsJson : IRenderOutput
    {
        public Task<Stream> Render(IDictionary<string, object> environment)
        {
            return Task.Factory.StartNew(() =>
            {
                var serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(environment.GetOutput()));

                var stream = (Stream)new MemoryStream(serialized);

                stream.Position = 0;

                return stream;
            });
        }
    }
}