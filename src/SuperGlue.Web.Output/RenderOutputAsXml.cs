using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SuperGlue.Web.Output
{
    public class RenderOutputAsXml : IRenderOutput
    {
        public Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput();

            if (output == null)
                return Task.FromResult<OutputRenderingResult>(null);

            var serializer = new XmlSerializer(output.GetType());

            var stream = new MemoryStream();

            serializer.Serialize(stream, output);

            return Task.FromResult(new OutputRenderingResult(stream, "application/xml"));
        }
    }
}