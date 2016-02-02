using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Components
{
    public interface IComponentSource
    {
        Task<IComponent> Find(IDictionary<string, object> environment, string component);
    }
}