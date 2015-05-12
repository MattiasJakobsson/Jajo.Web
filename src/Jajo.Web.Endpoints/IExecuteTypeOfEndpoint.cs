using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jajo.Web.Endpoints
{
    public interface IExecuteTypeOfEndpoint<in TEndpoint>
    {
        Task Execute(TEndpoint endpoint, IDictionary<string, object> environment);
    }
}