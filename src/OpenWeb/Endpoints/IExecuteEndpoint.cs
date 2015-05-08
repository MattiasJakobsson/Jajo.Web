using System.Reflection;
using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    public interface IExecuteEndpoint
    {
        Task Execute(MethodInfo endpointMethod, IOpenWebContext openWebContext);
    }
}