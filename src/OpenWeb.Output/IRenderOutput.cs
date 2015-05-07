using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace OpenWeb.Output
{
    public interface IRenderOutput
    {
        Task<Stream> Render(WebEnvironment environment);
    }

    public class RenderOutputAsJson : IRenderOutput
    {
        public Task<Stream> Render(WebEnvironment environment)
        {
            return Task.Factory.StartNew(() =>
            {
                var serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(environment.Output));

                var stream = (Stream)new MemoryStream(serialized);

                stream.Position = 0;

                return stream;
            });
        }
    }

    public class RenderOutputAsXml : IRenderOutput
    {
        public Task<Stream> Render(WebEnvironment environment)
        {
            return Task.Factory.StartNew(() =>
            {
                var serializer = new XmlSerializer(environment.Output.GetType());

                var stream = new MemoryStream();

                serializer.Serialize(stream, environment.Output);

                stream.Position = 0;

                return (Stream) stream;
            });
        }
    }
}