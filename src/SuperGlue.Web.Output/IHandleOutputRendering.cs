using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public interface IHandleOutputRendering
    {
        Task Render(IDictionary<string, object> environment);
    }
}