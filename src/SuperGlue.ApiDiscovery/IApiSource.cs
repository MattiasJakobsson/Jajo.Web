using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiSource
    {
        Task<IEnumerable<ApiDefinition>> Find(IDictionary<string, object> environment);
    }
}