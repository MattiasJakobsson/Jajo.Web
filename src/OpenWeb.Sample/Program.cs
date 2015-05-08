using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Microsoft.Owin.Hosting;
using OpenWeb.Endpoints;
using OpenWeb.ExceptionManagement;
using OpenWeb.ModelBinding;
using OpenWeb.Output;
using OpenWeb.Routing.Superscribe;
using OpenWeb.StructureMap;
using Owin;
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

            define.Get(x => x/"test", x => "").RouteTo(typeof(TestEndpoint).GetMethod("Query"));

            var rendererHandler = new HandleOutputRendering(new List<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>
            {
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
        public string Query()
        {
            return "Hello world!";
        }
    }
}
