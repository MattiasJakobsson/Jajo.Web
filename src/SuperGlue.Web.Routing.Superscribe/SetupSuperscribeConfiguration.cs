using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperGlue.Configuration;
using Superscribe;
using Superscribe.Engine;
using Superscribe.Models;
using Superscribe.Utils;

namespace SuperGlue.Web.Routing.Superscribe
{
    public class SetupSuperscribeConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.SuperscribeSetup", environment =>
            {
                var stringRouteParser = new StringRouteParser();

                var define = RouteEngineFactory.Create(new SuperscribeOptions
                {
                    StringRouteParser = stringRouteParser
                });

                environment.RegisterAll(typeof(ICheckIfRouteExists));

                environment[SuperscribeEnvironmentExtensions.SuperscribeConstants.Engine] = define;

                environment[SuperscribeEnvironmentExtensions.SuperscribeConstants.CreateRouteBuilderFunc] = (Func<IRouteBuilder>)(() => new RouteBuilder(environment.ResolveAll<ICheckIfRouteExists>(), stringRouteParser));

                environment[RouteExtensions.RouteConstants.CreateRouteFunc] = (Action<string, object, string[]>)((pattern, routeTo, methods) =>
                {
                    var routeBuilder = environment.CreateRouteBuilder();

                    routeBuilder.RestrictMethods(methods);
                    routeBuilder.AppendPattern(pattern);

                    //TODO:Handle parameters
                    routeBuilder.Build(define.Base, routeTo, new Dictionary<Type, Func<object, IDictionary<string, object>>>(), environment);
                });

                environment[RouteExtensions.RouteConstants.ReverseRoute] = (Func<object, string>)(endpoint =>
                {
                    var endpointRoute = environment.GetRouteForEndpoint(endpoint);

                    if (endpointRoute == null)
                        return "";

                    var patternBuilder = new StringBuilder();

                    var node = endpointRoute.Node;

                    while (node != null)
                    {
                        patternBuilder.Append("/");

                        var paramNode = node as ParamNode;

                        if (paramNode != null)
                        {
                            if (endpointRoute.Parameters.ContainsKey(paramNode.Name))
                                patternBuilder.Append(endpointRoute.Parameters[paramNode.Name]);
                        }
                        else
                        {
                            patternBuilder.Append(node.Template);
                        }

                        node = node.Edges.FirstOrDefault();
                    }

                    return patternBuilder.ToString();
                });
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}