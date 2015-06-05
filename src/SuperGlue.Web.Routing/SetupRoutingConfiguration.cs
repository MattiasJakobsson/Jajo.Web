using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Routing
{
    public class SetupRoutingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.RoutingSetup", environment =>
            {
                environment.RegisterAll(typeof(ICheckIfRouteExists));

                environment[RouteExtensions.RouteConstants.CreateRouteFunc] = (Action<string, object, string[]>)((pattern, routeTo, methods) =>
                {
                    var routeBuilder = environment.CreateRouteBuilder();

                    routeBuilder.RestrictMethods(methods);
                    routeBuilder.AppendPattern(pattern);

                    //TODO:Handle parameters
                    routeBuilder.Build(routeTo, new Dictionary<Type, Func<object, IDictionary<string, object>>>(), environment);
                });
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }

        public void Configure(SettingsConfiguration configuration)
        {
            var policies = configuration.WithSettings<RouteSettings>().GetPolicies();

            if(!policies.Any())
                policies = new ReadOnlyCollection<IRoutePolicy>(new List<IRoutePolicy>
                {
                    new QueryCommandMethodRoutePolicy(configuration.Environment.GetAssemblies())
                });

            foreach (var policy in policies)
            {
                var endpoints = policy.Build();

                foreach (var endpoint in endpoints)
                {
                    var routeBuilder = configuration.Environment.CreateRouteBuilder();

                    if (routeBuilder == null)
                        continue;

                    if (endpoint.HttpMethods.Any())
                        routeBuilder.RestrictMethods(endpoint.HttpMethods);

                    foreach (var urlPart in endpoint.UrlParts)
                        urlPart.AddToBuilder(routeBuilder);

                    routeBuilder.Build(endpoint.Destination, endpoint.RoutedParameters, configuration.Environment);
                }
            }
        }
    }
}