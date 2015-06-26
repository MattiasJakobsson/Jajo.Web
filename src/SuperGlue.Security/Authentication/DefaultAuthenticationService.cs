using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Security.Authentication
{
    public class DefaultAuthenticationService : IAuthenticationService
    {
        private readonly IEnumerable<IAuthenticationStrategy> _strategies;

        public DefaultAuthenticationService(IEnumerable<IAuthenticationStrategy> strategies)
        {
            _strategies = strategies;
        }

        public AuthenticationResult CheckAuthentication<TAuthenticationRequest>(TAuthenticationRequest authenticationRequest, IDictionary<string, object> environment) where TAuthenticationRequest : IAuthenticationRequest
        {
            var strategy = _strategies
                .OfType<IAuthenticationStrategy<TAuthenticationRequest>>()
                .FirstOrDefault();

            if(strategy == null)
                throw new InvalidOperationException(string.Format("No strategy defined for request of type: {0}", typeof(TAuthenticationRequest).Name));

            return strategy.CheckAuthentication(authenticationRequest, environment);
        }

        public void SignOut(IDictionary<string, object> environment)
        {
            foreach (var strategy in _strategies)
                strategy.SignOut(environment);
        }

        public void SigntOutBehalfOf(IDictionary<string, object> environment)
        {
            foreach (var strategy in _strategies)
                strategy.SignOut(environment);
        }

        public AuthenticationInformation GetUser(IDictionary<string, object> environment, string source = null)
        {
            foreach (var strategy in _strategies)
            {
                var information = strategy.GetUser(environment);

                if (information.IsAuthenticated && (string.IsNullOrEmpty(source) || information.User.Source == source))
                    return information;
            }

            return new AuthenticationInformation(null, null);
        }
    }
}