using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using OpenWeb.Authorization;
using OpenWeb.Configuration;
using OpenWeb.Endpoints;
using OpenWeb.ExceptionManagement;
using OpenWeb.Http;
using OpenWeb.ModelBinding;
using OpenWeb.Output;
using OpenWeb.Output.Spark;
using OpenWeb.RequestBranching;
using OpenWeb.Routing.Superscribe;
using OpenWeb.Routing.Superscribe.Conventional;
using OpenWeb.StructureMap;
using OpenWeb.UnitOfWork;
using OpenWeb.Validation;
using Owin;
using Spark;
using StructureMap;
using Superscribe.Engine;
using Superscribe.Models;

namespace OpenWeb.Sample
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var assemblies = new List<Assembly>
            {
                typeof (Program).Assembly
            };

            assemblies.AddRange(SubApplications.Init());

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

            var define = new ConventionalRoutingConfiguration(new List<IFilterEndpoints>
            {
                new QueryAndCommandEndpointFilter()
            }, new List<IRoutePolicy>
            {
                new DefaultRoutePolicy()
            }).Configure(assemblies);

            define.Get(x => x / "exception", x =>
            {
                return ((Action)(() =>
                {
                    throw new Exception("Exception");
                }));
            });

            define.Base.FinalFunctions.Add(new FinalFunction("GET", x =>
            {
                return ((Action)(() => ((RouteData)x).Environment.WriteToOutput("Test")));
            }));

            var links = (File.Exists("subapps.txt") ? File.ReadAllText("subapps.txt") : "").Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToList();

            var templateSource = new AggregatedTemplateSource(new EmbeddedTemplateSource(assemblies), new FileSystemTemplateSource(assemblies, new FileScanner(), links));

            var sparkViewEngine = new SparkViewEngine(new SparkSettings())
            {
                DefaultPageBaseType = typeof (OpenWebSparkView).FullName,
                ViewFolder = new OpenWebViewFolder(templateSource.FindTemplates()),
                PartialProvider = new OpenWebPartialProvider()
            };

            var rendererHandler = new HandleOutputRendering(new List<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>
            {
                new Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>(x => x.GetHeaders().Accept.Contains("text/html"), new RenderOutputUsingSpark(sparkViewEngine, templateSource)),
                new Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>(x => true, new RenderOutputAsJson())
            });

            var settings = new Dictionary<string, object>
            {
                {"openweb.StructureMap.Container", container},
                {"openweb.Superscribe.Engine", define},
                {"openweb.Spark.ViewEngine", sparkViewEngine}
            };

            var configurers = container.GetAllInstances<IRunAtConfigurationTime>();

            foreach (var configurer in configurers)
                configurer.Configure(settings);

            WebApp.Start("http://localhost:8020", x =>
                x.Use<BranchRequest>(new BranchRequestConfiguration()
                    .AddCase(y => y.GetException() != null, (AppFunc)x
                            .New()
                            .Use<SetStatusCode>(500)
                            .Use<RollbackUnitOfWork>()
                            .Use<HandledExceptionMiddleware>()
                            .Build(typeof(AppFunc)))
                    .AddCase(y => y.Get<bool>("openweb.AuthorizationFailed"), (AppFunc)x
                            .New()
                            .Use<SetStatusCode>(401)
                            .Use<HandleUnauthorizedMiddleware>()
                            .Build(typeof(AppFunc)))
                    .AddCase(y => !(y.Get<ValidationResult>("openweb.ValidationResult") ?? new ValidationResult(new List<ValidationResult.ValidationError>())).IsValid, (AppFunc)x
                            .New()
                            .Use<HandleValidationErrorMiddleware>()
                            .Build(typeof(AppFunc)))
                     .AddCase(y => y.GetOutput() == null, (AppFunc)x
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
                    .Use<RouteUsingSuperscribe>(define)
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>(rendererHandler));

            stopwatch.Stop();

            Console.WriteLine("Startup time: {0}ms", stopwatch.ElapsedMilliseconds);

            Console.ReadLine();
        }
    }

    public class OpenWebPartialProvider : IPartialProvider
    {
        public IEnumerable<string> GetPaths(string viewPath)
        {
            do
            {
                viewPath = Path.GetDirectoryName(viewPath);

                if(string.IsNullOrEmpty(viewPath))
                    break;

                yield return viewPath;
                yield return Path.Combine(viewPath, Constants.Shared);
            }
            while (!string.IsNullOrEmpty(viewPath));
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
            if(environment["owin.RequestPath"].ToString().Contains("invalid"))
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

            environment["openweb.Output"] = "Testing if statement";

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
