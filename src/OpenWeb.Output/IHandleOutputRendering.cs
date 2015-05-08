using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Output
{
    public interface IHandleOutputRendering
    {
        Task Render(IDictionary<string, object> environment);
    }
}