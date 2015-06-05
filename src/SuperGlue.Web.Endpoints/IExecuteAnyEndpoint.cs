using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    public interface IExecuteAnyEndpoint
    {
        Task Execute(IDictionary<string, object> environment, object endpoint);
    }
}