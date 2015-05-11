using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using OpenWeb.Authorization;
using OpenWeb.Configuration;
using OpenWeb.Diagnostics;
using OpenWeb.Endpoints;
using OpenWeb.ExceptionManagement;
using OpenWeb.Http;
using OpenWeb.ModelBinding;
using OpenWeb.Output;
using OpenWeb.Output.Spark;
using OpenWeb.PartialRequests;
using OpenWeb.RequestBranching;
using OpenWeb.Routing.Superscribe;
using OpenWeb.Routing.Superscribe.Conventional;
using OpenWeb.StructureMap;
using OpenWeb.UnitOfWork;
using OpenWeb.Validation;
using Owin;
using StructureMap;

namespace OpenWeb.Sample
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            WebApp.Start("http://localhost:8020", ConfigureApplication);

            stopwatch.Stop();

            Console.WriteLine("Startup time: {0}ms", stopwatch.ElapsedMilliseconds);

            Console.ReadLine();
        }

        private static void ConfigureApplication(IAppBuilder app)
        {
            var subApplications = SubApplications.Init().ToList();

            var assemblies = new List<Assembly>
            {
                typeof (Program).Assembly
            };

            assemblies.AddRange(subApplications.Select(x => x.Assembly));

            var container = new Container();

            container.Configure(x =>
            {
                x.Scan(y =>
                {
                    y.AssemblyContainingType(typeof(IExecuteTypeOfEndpoint<>));

                    foreach (var assembly in assemblies)
                        y.Assembly(assembly);

                    y.ConnectImplementationsToTypesClosing(typeof(IExecuteTypeOfEndpoint<>));
                    y.AddAllTypesOf<IOpenWebUnitOfWork>();
                    y.AddAllTypesOf<IRunAtConfigurationTime>();
                });
            });

            var modelBindingCollection = new ModelBinderCollection(new List<IModelBinder>());

            var settings = new Dictionary<string, object>
            {
                {"openweb.StructureMap.Container", container}
            };

            var define = ConventionalRoutingConfiguration.New()
                .UseEndpointFilterer(new QueryAndCommandEndpointFilter())
                .UseRoutePolicy(new DefaultRoutePolicy())
                .Configure(assemblies, settings);

            var templateSource = new AggregatedTemplateSource(new EmbeddedTemplateSource(assemblies), new FileSystemTemplateSource(assemblies, new FileScanner(), subApplications.Select(x => x.Path)));

            var rendererHandler = OutputRendererBuilder.New()
                .When(x => x.GetHeaders().Accept.Contains("text/html")).UseRenderer(RenderOutputUsingSpark.Configure(templateSource, settings))
                .When(x => true).UseRenderer(new RenderOutputAsJson())
                .Build();

            var configurers = container.GetAllInstances<IRunAtConfigurationTime>();

            foreach (var configurer in configurers)
                configurer.Configure(settings);

            var partialFlow = (AppFunc)app.New()
                    .Use<MeasureInner>(new MeasureInnerOptions((time, environment) => Console.WriteLine("Executed partial in {0}ms.", (int)time.TotalMilliseconds)))
                    .Use<HandleExceptions>()
                    .Use<AuthorizeRequest>(new AuthorizeRequestOptions().WithAuthorizer(new TestAuthorizer()))
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>(rendererHandler)
                    .Build(typeof(AppFunc));

            Partials.Initialize(partialFlow);

            app.Use<MeasureInner>(new MeasureInnerOptions((time, environment) => Console.WriteLine("Executed url: {0} in {1}ms.", environment["owin.RequestPath"].ToString(), (int)time.TotalMilliseconds)))
                .Use<RedirectToCorrectUrl>(new RedirectToCorrectUrlOptions(y => y.ToLower()))
                .Use<BranchRequest>(new BranchRequestConfiguration()
                    .AddCase(y => y.GetException() != null, (AppFunc)app
                            .New()
                            .Use<SetStatusCode>(500)
                            .Use<RollbackUnitOfWork>()
                            .Use<HandledExceptionMiddleware>()
                            .Build(typeof(AppFunc)))
                    .AddCase(y => y.Get<bool>("openweb.AuthorizationFailed"), (AppFunc)app
                            .New()
                            .Use<SetStatusCode>(401)
                            .Use<HandleUnauthorizedMiddleware>()
                            .Build(typeof(AppFunc)))
                    .AddCase(y => !(y.Get<ValidationResult>("openweb.ValidationResult") ?? new ValidationResult(new List<ValidationResult.ValidationError>())).IsValid, (AppFunc)app
                            .New()
                            .Use<HandleValidationErrorMiddleware>()
                            .Build(typeof(AppFunc)))
                     .AddCase(y => y.GetOutput() == null, (AppFunc)app
                            .New()
                            .Use<SetStatusCode>(404)
                            .Use<HandleNotFoundMiddleware>()
                            .Build(typeof(AppFunc))))
                .Use<NestedStructureMapContainer>(container)
                .Use<HandleExceptions>()
                .Use<BindModels>(modelBindingCollection)
                .Use<HandleUnitOfWork>()
                .Use<AuthorizeRequest>(new AuthorizeRequestOptions().WithAuthorizer(new TestAuthorizer()))
                .Use<ValidateRequest>(new ValidateRequestOptions().UsingValidator(new TestValidator()))
                .Use<RouteUsingSuperscribe>(new RouteUsingSuperscribeOptions(define, settings))
                .Use<ExecuteEndpoint>()
                .Use<RenderOutput>(rendererHandler);
        }
    }

    public class TestAuthorizer : IAuthorizeRequest
    {
        public bool IsAuthorized(IEnumerable<AuthenticationToken> tokens, IDictionary<string, object> environment)
        {
            return !environment["owin.RequestPath"].ToString().Contains("unauthorized");
        }
    }

    public class TestValidator : IValidateRequest
    {
        public ValidationResult Validate(IDictionary<string, object> environment)
        {
            if (environment["owin.RequestPath"].ToString().Contains("invalid"))
                return new ValidationResult(new List<ValidationResult.ValidationError>
                {
                    new ValidationResult.ValidationError("Key", "Error")
                });

            return new ValidationResult(new List<ValidationResult.ValidationError>());
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
            var exception = environment.Get<Exception>("openweb.Exception");

            await environment.WriteToOutput(exception.Message);
            await environment.WriteToOutput(exception.StackTrace);

            environment["openweb.Output"] = exception.Message;

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
            await environment.WriteToOutput("Not found!");

            environment["openweb.Output"] = "Not found!";

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
            var validationResult = environment.Get<ValidationResult>("openweb.ValidationResult");

            foreach (var error in validationResult.Errors)
                result.AppendFormat("Error {0}: {1}<br/>", error.Key, error.Message);

            await environment.WriteToOutput(result.ToString());

            environment["openweb.Output"] = result.ToString();

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
            await environment.WriteToOutput("Unauthorized");

            environment["openweb.Output"] = "Unauthorized";

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
