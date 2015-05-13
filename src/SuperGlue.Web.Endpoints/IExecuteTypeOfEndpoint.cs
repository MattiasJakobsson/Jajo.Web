using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public interface IExecuteTypeOfEndpoint<in TEndpoint>
    {
        Task Execute(TEndpoint endpoint, IDictionary<string, object> environment);
    }
}