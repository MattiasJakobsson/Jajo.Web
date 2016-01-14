using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Security.Authorization
{
    public interface IFindRequiredAuthorizationInformationFromRequest
    {
        Task<IEnumerable<IAuthorizationInformation>> FindFor(IDictionary<string, object> environment);
    }
}