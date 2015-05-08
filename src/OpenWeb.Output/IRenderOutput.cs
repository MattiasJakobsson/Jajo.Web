using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenWeb.Output
{
    public interface IRenderOutput
    {
        Task<Stream> Render(IDictionary<string, object> environment);
    }
}