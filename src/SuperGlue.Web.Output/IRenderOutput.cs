using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public interface IRenderOutput
    {
        Task<OutputRenderingResult> Render(IDictionary<string, object> environment);
    }
}