using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Owin.Hosting;
using OpenWeb.Endpoints;
using OpenWeb.ExceptionManagement;
using OpenWeb.ModelBinding;
using OpenWeb.Output;
using OpenWeb.Output.Spark;
using OpenWeb.Routing.Superscribe;
using OpenWeb.StructureMap;
using Owin;
using Spark;
using StructureMap;
using Superscribe.Engine;

namespace OpenWeb.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            var modelBindingCollection = new ModelBinderCollection(new List<IModelBinder>());
            var define = RouteEngineFactory.Create();

            define.Get(x => x / "test", x => typeof(TestEndpoint).GetMethod("Query"));

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
                x.Use<StructureMapNestedContainerMiddleware>(container)
                    .Use<OpenWebExceptionManagementMiddleware>()
                    .Use<OpenWebModelBindingMiddleware>(modelBindingCollection)
                    .Use<OpenWebSuperscribeMiddleware>(define)
                    .Use<OpenWebEndpointsMiddleware>()
                    .Use<OpenWebOutputMiddleware>(rendererHandler));

            Console.ReadLine();
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
