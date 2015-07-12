using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiRegistry
    {
        Task Register(IDictionary<string, object> environment, params ApiDefinition[] apis);
        Task<ApiDefinition> Find(IDictionary<string, object> environment, string name);
    }
}