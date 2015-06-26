using System.Collections.Generic;
using SuperGlue.Security.Authorization;

namespace SuperGlue.Web.RouteInputValidator
{
    public interface IAmSecuredBy
    {
        IEnumerable<IAuthorizationInformation> GetSecuredBy();
    }
}