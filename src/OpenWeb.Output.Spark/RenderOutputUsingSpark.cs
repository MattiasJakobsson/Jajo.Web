using System;
using System.IO;
using System.Threading.Tasks;
using OpenWeb.Endpoints;

namespace OpenWeb.Output.Spark
{
    public class RenderOutputUsingSpark : IRenderOutput
    {
        public Task<Stream> Render(IOpenWebContext context)
        {
            throw new NotImplementedException();
        }
    }
}