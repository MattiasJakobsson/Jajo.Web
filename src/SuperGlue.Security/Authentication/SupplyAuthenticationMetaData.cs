using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.MetaData;

namespace SuperGlue.Security.Authentication
{
    public class SupplyAuthenticationMetaData : ISupplyRequestMetaData
    {
        private readonly IAuthenticationService _authenticationService;

        public SupplyAuthenticationMetaData(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public bool CanHandleChain(string chain)
        {
            return true;
        }

        public Task<IDictionary<string, object>> GetMetaData(IDictionary<string, object> environment)
        {
            var metaData = new Dictionary<string, object>();

            var users = _authenticationService.GetUsers(environment);

            foreach (var user in users)
            {
                metaData[string.Format("Authentications.{0}.User", user.User.Source)] = user.User.GetClaimsString();

                if (user.IsBehalfOf)
                    metaData[string.Format("Authentications.{0}.OnBehalfOf", user.User.Source)] = user.OnBehalfOf.GetClaimsString();
            }

            return Task.FromResult<IDictionary<string, object>>(metaData);
        }
    }
}