using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public interface IRenderToOutput
    {
        Task Render(IDictionary<string, object> environment);
    }
}