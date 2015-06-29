using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Routing
{
    public class SetupRoutingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.RoutingSetup", environment =>
            {
                environment.RegisterAll(typeof(ICheckIfRouteExists));

                environment[RouteExtensions.RouteConstants.CreateRouteFunc] = (Action<string, object, IDictionary<Type, Func<object, IDictionary<string, object>>>, string[]>)((pattern, routeTo, inputParameters, methods) =>
                {
                    var routeBuilder = environment.CreateRouteBuilder();

                    routeBuilder.RestrictMethods(methods);
                    routeBuilder.AppendPattern(pattern);

                    routeBuilder.Build(routeTo, inputParameters, environment);
                });
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() =>
            {
                var policies = configuration.WithSettings<RouteSettings>().GetPolicies();

                if (!policies.Any())
                    policies = new ReadOnlyCollection<IRoutePolicy>(new List<IRoutePolicy>
                    {
                        new QueryCommandMethodRoutePolicy(configuration.Settings.GetAssemblies())
                    });

                foreach (var policy in policies)
                {
                    var endpoints = policy.Build();

                    foreach (var endpoint in endpoints)
                    {
                        var routeBuilder = configuration.Settings.CreateRouteBuilder();

                        if (routeBuilder == null)
                            continue;

                        if (endpoint.HttpMethods.Any())
                            routeBuilder.RestrictMethods(endpoint.HttpMethods);

                        foreach (var urlPart in endpoint.UrlParts)
                            urlPart.AddToBuilder(routeBuilder);

                        routeBuilder.Build(endpoint.Destination, endpoint.RoutedParameters, configuration.Settings);
                    }
                }
            });
        }
    }
}