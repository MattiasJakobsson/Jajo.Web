using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.ApiDiscovery
{
    public interface IApiRegistry
    {
        Task Register(IDictionary<string, object> environment, params ApiDefinition[] apis);
        IApiQuery Start(IDictionary<string, object> environment, string apiName);
    }
}