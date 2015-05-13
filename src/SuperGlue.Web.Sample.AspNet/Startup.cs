using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using SuperGlue.Web.Authorization;
using SuperGlue.Web.Configuration;
using SuperGlue.Web.Endpoints;
using SuperGlue.Web.ExceptionManagement;
using SuperGlue.Web.Http;
using SuperGlue.Web.ModelBinding;
using SuperGlue.Web.Output;
using SuperGlue.Web.Output.Spark;
using SuperGlue.Web.PartialRequests;
using SuperGlue.Web.RequestBranching;
using SuperGlue.Web.Routing.Superscribe;
using SuperGlue.Web.Routing.Superscribe.Conventional;
using SuperGlue.Web.Sample.AspNet;
using SuperGlue.Web.StructureMap;
using SuperGlue.Web.UnitOfWork;
using SuperGlue.Web.Validation;
using Owin;
using StructureMap;

[assembly: OwinStartup(typeof(Startup))]
namespace SuperGlue.Web.Sample.AspNet
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var subApplications = SubApplications.Init().ToList();

            var assemblies = new List<Assembly>
            {
                typeof (Startup).Assembly
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
                    y.AddAllTypesOf<ISuperGlueUnitOfWork>();
                    y.AddAllTypesOf<IRunAtConfigurationTime>();
                });
            });

            var modelBindingCollection = new ModelBinderCollection(new List<IModelBinder>());

            var settings = new Dictionary<string, object>
            {
                {"superglue.StructureMap.Container", container}
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
                    .Use<HandleExceptions>()
                    .Use<AuthorizeRequest>(new AuthorizeRequestOptions().WithAuthorizer(new TestAuthorizer()))
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>(rendererHandler)
                    .Build(typeof(AppFunc));

            Partials.Initialize(partialFlow);

            app.Use<RedirectToCorrectUrl>(new RedirectToCorrectUrlOptions(y => y.ToLower()))
                .Use<BranchRequest>(new BranchRequestConfiguration()
                    .AddCase(y => y.GetException() != null, (AppFunc)app
                            .New()
                            .Use<SetStatusCode>(500)
                            .Use<RollbackUnitOfWork>()
                            .Use<HandledExceptionMiddleware>()
                            .Build(typeof(AppFunc)))
                    .AddCase(y => y.Get<bool>("superglue.AuthorizationFailed"), (AppFunc)app
                            .New()
                            .Use<SetStatusCode>(401)
                            .Use<HandleUnauthorizedMiddleware>()
                            .Build(typeof(AppFunc)))
                    .AddCase(y => !(y.Get<ValidationResult>("superglue.ValidationResult") ?? new ValidationResult(new List<ValidationResult.ValidationError>())).IsValid, (AppFunc)app
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

    public class TestMiddleware
    {
        private readonly AppFunc _next;

        public TestMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await environment.WriteToOutput("Testing if statement");

            environment["superglue.Output"] = "Testing if statement";

            await _next(environment);
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

            await environment.WriteToOutput(exception.Message);

            environment["superglue.Output"] = exception.Message;

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

            environment["superglue.Output"] = "Not found!";

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

            await environment.WriteToOutput(result.ToString());

            environment["superglue.Output"] = result.ToString();

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

            environment["superglue.Output"] = "Unauthorized";

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