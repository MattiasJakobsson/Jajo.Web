using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using Superscribe;
using Superscribe.Engine;
using Superscribe.Models;
using Superscribe.Utils;

namespace SuperGlue.Web.Routing.Superscribe
{
    public class SetupSuperscribeConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.RoutingImplimentationSetup", environment =>
            {
                var stringRouteParser = new StringRouteParser();

                var define = RouteEngineFactory.Create(new SuperscribeOptions
                {
                    StringRouteParser = stringRouteParser
                });

                environment[SuperscribeEnvironmentExtensions.SuperscribeConstants.Engine] = define;

                environment[RoutingEnvironmentExtensions.RouteConstants.CreateRouteBuilderFunc] = (Func<IRouteBuilder>)(() => new SuperscribeRouteBuilder(environment.ResolveAll<ICheckIfRouteExists>(), stringRouteParser, environment));

                environment[RouteExtensions.RouteConstants.EndpointFromInput] = (Func<object, object>) (environment.GetEndpointFor);

                environment[RouteExtensions.RouteConstants.InputsForEndpoint] = (Func<object, IEnumerable<Type>>) (x =>
                {
                    var route = environment.GetRouteForEndpoint(x);

                    return route == null ? new List<Type>() : route.InputTypes;
                });

                environment[RouteExtensions.RouteConstants.ReverseRoute] = (Func<object, string>)(endpoint =>
                {
                    var endpointRoute = environment.GetRouteForEndpoint(endpoint);

                    if (endpointRoute == null)
                        return "";

                    var patternParts = new List<string>();

                    var node = endpointRoute.Node;

                    var appendedParameters = new List<string>();

                    while (node != null)
                    {
                        var paramNode = node as ParamNode;

                        if (paramNode != null)
                        {
                            if (endpointRoute.Parameters.ContainsKey(paramNode.Name))
                            {
                                patternParts.Add(endpointRoute.Parameters[paramNode.Name].ToString());
                                appendedParameters.Add(paramNode.Name);
                            }
                        }
                        else
                        {
                            patternParts.Add(node.Template);
                        }

                        patternParts.Add("/");

                        node = node.Parent;
                    }

                    var missingParameters = endpointRoute.Parameters.Where(x => !appendedParameters.Contains(x.Key));

                    var patternBuilder = new StringBuilder();
                    patternParts.Reverse();

                    foreach (var part in patternParts)
                        patternBuilder.Append(part);

                    var parameterSplitter = "?";

                    foreach (var missingParameter in missingParameters)
                    {
                        patternBuilder.AppendFormat("{0}{1}={2}", parameterSplitter, missingParameter.Key, missingParameter.Value);
                        parameterSplitter = "&";
                    }

                    return patternBuilder.ToString();
                });

                return Task.CompletedTask;
            }, "superglue.RoutingSetup");
        }
    }
}