using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using OpenWeb.Authorization;
using OpenWeb.Endpoints;
using OpenWeb.ExceptionManagement;
using OpenWeb.ModelBinding;
using OpenWeb.Output;
using OpenWeb.Output.Spark;
using OpenWeb.Owin;
using OpenWeb.Routing.Superscribe;
using OpenWeb.StructureMap;
using OpenWeb.UnitOfWork;
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
            var container = new Container();

            container.Configure(x =>
            {
                x.Scan(y =>
                {
                    y.AssemblyContainingType(typeof(IExecuteTypeOfEndpoint<>));
                    y.AssemblyContainingType<Program>();

                    y.ConnectImplementationsToTypesClosing(typeof(IExecuteTypeOfEndpoint<>));
                    y.AddAllTypesOf<IOpenWebUnitOfWork>();
                });
            });

            var modelBindingCollection = new ModelBinderCollection(new List<IModelBinder>());
            var define = RouteEngineFactory.Create();

            define.Get(x => x / "test", x => typeof(TestEndpoint).GetMethod("Query"));
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

            var assemblies = new List<Assembly>
            {
                typeof (Program).Assembly
            };

            var templateSource = new AggregatedTemplateSource(new EmbeddedTemplateSource(assemblies), new FileSystemTemplateSource(assemblies, new FileScanner()));

            var sparkViewEngine = new SparkViewEngine(new SparkSettings())
            {
                DefaultPageBaseType = typeof(OpenWebSparkView).FullName,
                ViewFolder = new OpenWebViewFolder(templateSource.FindTemplates())
            };

            var rendererHandler = new HandleOutputRendering(new List<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>
            {
                new Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>(x => x.GetHeaders().Accept.Contains("text/html"), new RenderOutputUsingSpark(sparkViewEngine, templateSource)),
                new Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>(x => true, new RenderOutputAsJson())
            });

            WebApp.Start("http://localhost:8020", x =>
                x.Use<SpecialCaseMiddleware>(new SpecialCaseConfiguration()
                    .AddCase(y => y.GetException() != null, (AppFunc)x
                            .New()
                            .Use<SetStatusCodeMiddleware>(500)
                            .Use<RollbackTransactionsMiddleware>()
                            .Use<HandledExceptionMiddleware>()
                            .Build(typeof(AppFunc)))
                    .AddCase(y => y.Get<bool>("openweb.AuthorizationFailed"), (AppFunc)x
                            .New()
                            .Use<SetStatusCodeMiddleware>(401)
                            .Use<HandleUnauthorizedMiddleware>()
                            .Build(typeof(AppFunc)))
                     .AddCase(y => y.GetOutput() == null, (AppFunc)x
                            .New()
                            .Use<SetStatusCodeMiddleware>(404)
                            .Use<HandleNotFoundMiddleware>()
                            .Build(typeof(AppFunc))))
                    .Use<StructureMapNestedContainerMiddleware>(container)
                    .Use<OpenWebExceptionManagementMiddleware>()
                    .Use<OpenWebModelBindingMiddleware>(modelBindingCollection)
                    .Use<OpenWebUnitOfWorkMiddleware>()
                    .Use<OpenWebAuthorizationMiddleware>(new OpenWebAuthorizationOptions().WithAuthorizer(new TestAuthorizer()))
                    .Use<OpenWebSuperscribeMiddleware>(define)
                    .If(y => y["owin.RequestPath"].ToString().Contains("asd"), y => y.Use<TestMiddleware>())
                    .Use<OpenWebEndpointsMiddleware>()
                    .Use<OpenWebOutputMiddleware>(rendererHandler));

            Console.ReadLine();
        }
    }

    public class TestAuthorizer : IAuthorizeRequest
    {
        public bool IsAuthorized(IEnumerable<AuthenticationToken> tokens, IDictionary<string, object> environment)
        {
            return !environment["owin.RequestPath"].ToString().Contains("unauthorized");
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

            environment.SetOutput("Testing if statement");

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
            await environment.WriteToOutput("Not found!");

            environment.SetOutput("Not found!");

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
