using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SuperGlue.Security.Authentication;
using SuperGlue.Security.Authorization;
using SuperGlue.Web.Validation;

namespace SuperGlue.Web.Sample
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class TestAuthorizer : IAuthorizeRequest
    {
        public bool IsAuthorized(IEnumerable<AuthenticationToken> tokens, IDictionary<string, object> environment)
        {
            return !environment["owin.RequestPath"].ToString().Contains("unauthorized");
        }
    }

    public class HandledExceptionMiddleware
    {
        private readonly AppFunc _next;

        public HandledExceptionMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var exception = environment.Get<Exception>("superglue.Exception");

            await environment.GetResponse().Write(exception.Message);
            await environment.GetResponse().Write(exception.StackTrace);

            environment.SetOutput(exception.Message);

            await _next(environment);
        }
    }

    public class HandleNotFoundMiddleware
    {
        private readonly AppFunc _next;

        public HandleNotFoundMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await environment.GetResponse().Write("Not found!");

            environment.SetOutput("Not found!");

            await _next(environment);
        }
    }

    public class HandleValidationErrorMiddleware
    {
        private readonly AppFunc _next;

        public HandleValidationErrorMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var result = new StringBuilder();
            var validationResult = environment.Get<ValidationResult>("superglue.ValidationResult");

            foreach (var error in validationResult.Errors)
                result.AppendFormat("Error {0}: {1}<br/>", error.Key, error.Message);

            await environment.GetResponse().Write(result.ToString());

            environment.SetOutput(result.ToString());

            await _next(environment);
        }
    }

    public class HandleUnauthorizedMiddleware
    {
        private readonly AppFunc _next;

        public HandleUnauthorizedMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await environment.GetResponse().Write("Unauthorized");

            environment.SetOutput("Unauthorized");

            await _next(environment);
        }
    }

    public class TestEndpoint
    {
        public TestEndpointQueryResult Query()
        {
            return new TestEndpointQueryResult("Hello world!");
        }
    }

    public class TestEndpointQueryResult
    {
        public TestEndpointQueryResult(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}
