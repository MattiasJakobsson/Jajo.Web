using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Authorization
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class AuthorizeRequest
    {
        private readonly AppFunc _next;
        private readonly AuthorizeRequestOptions _options;

        public AuthorizeRequest(AppFunc next, AuthorizeRequestOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var tokens = _options.AuthenticationTokenSources.Select(x => x.GetToken(environment)).ToList();

            var isAuthorized = _options.AuthorizeRequests.Any(x => x.IsAuthorized(tokens, environment));

            if (isAuthorized)
                await _next(environment);
            else
                environment["superglue.AuthorizationFailed"] = true;
        }
    }

    public class AuthorizeRequestOptions
    {
        private readonly IList<IAuthenticationTokenSource> _authenticationTokenSources = new List<IAuthenticationTokenSource>();
        private readonly IList<IAuthorizeRequest> _authorizeRequests = new List<IAuthorizeRequest>();

        public IReadOnlyCollection<IAuthenticationTokenSource> AuthenticationTokenSources { get { return new ReadOnlyCollection<IAuthenticationTokenSource>(_authenticationTokenSources); } }
        public IReadOnlyCollection<IAuthorizeRequest> AuthorizeRequests { get { return new ReadOnlyCollection<IAuthorizeRequest>(_authorizeRequests); } }

        public AuthorizeRequestOptions WithTokenSource(IAuthenticationTokenSource source)
        {
            _authenticationTokenSources.Add(source);

            return this;
        }

        public AuthorizeRequestOptions WithAuthorizer(IAuthorizeRequest authorizeRequest)
        {
            _authorizeRequests.Add(authorizeRequest);

            return this;
        }
    }
}