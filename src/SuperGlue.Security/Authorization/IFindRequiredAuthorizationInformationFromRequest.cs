using System.Collections.Generic;

namespace SuperGlue.Security.Authorization
{
    public interface IFindRequiredAuthorizationInformationFromRequest
    {
        IEnumerable<IAuthorizationInformation> FindFor(IDictionary<string, object> environment);
    }
}