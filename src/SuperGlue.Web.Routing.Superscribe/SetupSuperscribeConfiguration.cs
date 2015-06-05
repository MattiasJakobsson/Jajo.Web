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

                environment[SuperscribeEnvironmentExtensions.SuperscribeConstants.Engine] = define;

                environment[RoutingEnvironmentExtensions.RouteConstants.CreateRouteBuilderFunc] = (Func<IRouteBuilder>)(() => new SuperscribeRouteBuilder(environment.ResolveAll<ICheckIfRouteExists>(), stringRouteParser));

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
            }, "superglue.RoutingSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }

        public void Configure(SettingsConfiguration configuration)
        {
            
        }
    }
}