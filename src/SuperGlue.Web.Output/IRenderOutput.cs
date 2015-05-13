using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public interface IRenderOutput
    {
        Task<Stream> Render(IDictionary<string, object> environment);
    }
}