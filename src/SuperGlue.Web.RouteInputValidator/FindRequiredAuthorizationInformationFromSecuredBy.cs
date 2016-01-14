using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Security.Authorization;

namespace SuperGlue.Web.RouteInputValidator
{
    public class FindRequiredAuthorizationInformationFromSecuredBy : IFindRequiredAuthorizationInformationFromRequest
    {
        public async Task<IEnumerable<IAuthorizationInformation>> FindFor(IDictionary<string, object> environment)
        {
            var endpoint = environment.GetRouteInformation().RoutedTo;

            if(endpoint == null)
                return new List<IAuthorizationInformation>();

            var authorizationInformations = new List<IAuthorizationInformation>();

            foreach (var input in environment.GetInputsForEndpoint(environment))
            {
                var securedBy = await environment.Bind(input) as IAmSecuredBy;

                if(securedBy != null)
                    authorizationInformations.AddRange(securedBy.GetSecuredBy());
            }

            return authorizationInformations;
        }
    }
}