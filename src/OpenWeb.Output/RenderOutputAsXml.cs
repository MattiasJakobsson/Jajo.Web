using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenWeb.Output
{
    public class RenderOutputAsXml : IRenderOutput
    {
        public Task<Stream> Render(IDictionary<string, object> environment)
        {
            return Task.Factory.StartNew(() =>
            {
                var output = environment.GetOutput();

                var serializer = new XmlSerializer(output.GetType());

                var stream = new MemoryStream();

                serializer.Serialize(stream, output);

                stream.Position = 0;

                return (Stream) stream;
            });
        }
    }
}