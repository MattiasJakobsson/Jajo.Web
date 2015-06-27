using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Security.Authentication
{
    public class DefaultAuthenticationService : IAuthenticationService
    {
        private readonly IEnumerable<IAuthenticationStrategy> _strategies;
        private readonly IEnumerable<IAuthenticationTokenSource> _authenticationTokenSources;

        public DefaultAuthenticationService(IEnumerable<IAuthenticationStrategy> strategies, IEnumerable<IAuthenticationTokenSource> authenticationTokenSources)
        {
            _strategies = strategies;
            _authenticationTokenSources = authenticationTokenSources;
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

        public void SignOut(IDictionary<string, object> environment, string source = null)
        {
            foreach (var tokenSource in _authenticationTokenSources.Where(x => string.IsNullOrEmpty(source) || x.Name == source))
                tokenSource.SignOut(environment);
        }

        public void SigntOutBehalfOf(IDictionary<string, object> environment, string source = null)
        {
            foreach (var tokenSource in _authenticationTokenSources.Where(x => string.IsNullOrEmpty(source) || x.Name == source))
                tokenSource.SignOut(environment);
        }

        public AuthenticationInformation GetUser(IDictionary<string, object> environment, string source = null)
        {
            foreach (var tokenSource in _authenticationTokenSources.Where(x => string.IsNullOrEmpty(source) || x.Name == source))
            {
                var information = tokenSource.GetUser(environment);

                if (information.IsAuthenticated)
                    return information;
            }

            return new AuthenticationInformation(null, null);
        }

        public void SetAuthenticatedUser(AuthenticationToken token, IDictionary<string, object> environment)
        {
            foreach (var tokenSource in _authenticationTokenSources.Where(x => string.IsNullOrEmpty(token.Source) || x.Name == token.Source))
            {
                tokenSource.SetAuthenticatedUser(token, environment);
            }
        }

        public void SetOnBehalfOf(AuthenticationToken token, IDictionary<string, object> environment)
        {
            foreach (var tokenSource in _authenticationTokenSources.Where(x => string.IsNullOrEmpty(token.Source) || x.Name == token.Source))
            {
                tokenSource.SetOnBehalfOf(token, environment);
            }
        }
    }
}