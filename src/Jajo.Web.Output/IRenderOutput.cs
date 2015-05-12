using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Jajo.Web.Output
{
    public interface IRenderOutput
    {
        Task<Stream> Render(IDictionary<string, object> environment);
    }
}