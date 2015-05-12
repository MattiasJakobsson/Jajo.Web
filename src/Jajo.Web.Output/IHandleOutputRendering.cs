using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jajo.Web.Output
{
    public interface IHandleOutputRendering
    {
        Task Render(IDictionary<string, object> environment);
    }
}