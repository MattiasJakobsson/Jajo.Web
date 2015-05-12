using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Jajo.Web.Endpoints
{
    public interface IExecuteEndpoint
    {
        Task Execute(MethodInfo endpointMethod, IDictionary<string, object> environment);
    }
}