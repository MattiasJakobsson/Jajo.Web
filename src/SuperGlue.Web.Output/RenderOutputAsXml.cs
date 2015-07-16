using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SuperGlue.Web.Output
{
    public class RenderOutputAsXml : IRenderOutput
    {
        public async Task<OutputRenderingResult> Render(IDictionary<string, object> environment)
        {
            var output = environment.GetOutput();

            if (output == null)
                return null;

            var serializer = new XmlSerializer(output.GetType());

            var stream = new MemoryStream();

            serializer.Serialize(stream, output);

            stream.Position = 0;
            var content = await new StreamReader(stream).ReadToEndAsync();

            return new OutputRenderingResult(content, "application/xml");
        }
    }
}