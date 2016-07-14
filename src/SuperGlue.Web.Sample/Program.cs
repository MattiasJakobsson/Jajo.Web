using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Web.Validation;

namespace SuperGlue.Web.Sample
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Program
    {
        public static void Main()
        {
            var bootstrapper = SuperGlueBootstrapper.Find();

            bootstrapper.StartApplications(new Dictionary<string, object>(), "local").Wait();

            Console.Read();
        }
    }

    public class HandledExceptionMiddleware
    {
        private readonly AppFunc _next;

        public HandledExceptionMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var exception = environment.Get<Exception>("superglue.Exception");

            await environment.GetResponse().Write(exception.Message).ConfigureAwait(false);
            await environment.GetResponse().Write(exception.StackTrace).ConfigureAwait(false);

            environment.SetOutput(exception.Message);

            await _next(environment).ConfigureAwait(false);
        }
    }

    public class HandleNotFoundMiddleware
    {
        private readonly AppFunc _next;

        public HandleNotFoundMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await environment.GetResponse().Write("Not found!").ConfigureAwait(false);

            environment.SetOutput("Not found!");

            await _next(environment).ConfigureAwait(false);
        }
    }

    public class HandleValidationErrorMiddleware
    {
        private readonly AppFunc _next;

        public HandleValidationErrorMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var result = new StringBuilder();
            var validationResult = environment.Get<ValidationResult>("superglue.ValidationResult");

            foreach (var error in validationResult.Errors)
                result.AppendFormat("Error {0}: {1}<br/>", error.Key, error.Message);

            await environment.GetResponse().Write(result.ToString()).ConfigureAwait(false);

            environment.SetOutput(result.ToString());

            await _next(environment).ConfigureAwait(false);
        }
    }

    public class HandleUnauthorizedMiddleware
    {
        private readonly AppFunc _next;

        public HandleUnauthorizedMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await environment.GetResponse().Write("Unauthorized").ConfigureAwait(false);

            environment.SetOutput("Unauthorized");

            await _next(environment).ConfigureAwait(false);
        }
    }

    public class TestEndpoint
    {
        public TestEndpointQueryResult Query(TestEndpointQueryInput input)
        {
            return new TestEndpointQueryResult("Hello world!");
        }
    }

    public class TestEndpointQueryInput
    {
        public string TestInput { get; set; }
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
