using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.Components
{
    public interface IComponent
    {
        Task RenderTo(IDictionary<string, object> environment, dynamic data, TextWriter writer);
    }
}