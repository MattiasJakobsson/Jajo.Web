using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenWeb.Output.Spark
{
    public class RenderOutputUsingSpark : IRenderOutput
    {
        public Task<Stream> Render(IDictionary<string, object> environment)
        {
            throw new NotImplementedException();
        }
    }
}