using System.Collections.Generic;
using System.Linq;
using SuperGlue.Security.Authorization;

namespace SuperGlue.Web.RouteInputValidator
{
    public class FindRequiredAuthorizationInformationFromSecuredBy : IFindRequiredAuthorizationInformationFromRequest
    {
        public IEnumerable<IAuthorizationInformation> FindFor(IDictionary<string, object> environment)
        {
            var endpoint = environment.GetRouteInformation().RoutedTo;

            if(endpoint == null)
                return new List<IAuthorizationInformation>();

            var inputs = environment.GetInputsForEndpoint(environment).Select(x => environment.Bind(x).Result).OfType<IAmSecuredBy>();

            return inputs.SelectMany(x => x.GetSecuredBy());
        }
    }
}