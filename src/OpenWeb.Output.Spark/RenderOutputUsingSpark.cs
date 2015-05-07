using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenWeb.Output.Spark
{
    public class RenderOutputUsingSpark : IRenderOutput
    {
        public Task<Stream> Render(WebEnvironment environment)
        {
            throw new NotImplementedException();
        }
    }
}