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
        private static readonly ICollection<Tuple<string, object, IDictionary<Type, Func<object, IDictionary<string, object>>>, string[]>> RoutesToConfigure = new List<Tuple<string, object, IDictionary<Type, Func<object, IDictionary<string, object>>>, string[]>>();
        private static bool shouldDelayRouteConfiguration = true;

        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.RoutingSetup", environment =>
            {
                environment.RegisterAll(typeof(ICheckIfRouteExists));

                environment[RouteExtensions.RouteConstants.CreateRouteFunc] = (Action<string, object, IDictionary<Type, Func<object, IDictionary<string, object>>>, string[]>)((pattern, routeTo, inputParameters, methods) =>
                {
                    var configuration = new Tuple<string, object, IDictionary<Type, Func<object, IDictionary<string, object>>>, string[]>(pattern, routeTo, inputParameters, methods);

                    if (shouldDelayRouteConfiguration)
                        RoutesToConfigure.Add(configuration);
                    else
                        AddRoute(configuration, environment);
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup", configureAction: async configuration =>
            {
                shouldDelayRouteConfiguration = false;

                foreach (var routeToConfigure in RoutesToConfigure)
                    AddRoute(routeToConfigure, configuration.Settings);

                RoutesToConfigure.Clear();

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

                        await routeBuilder.Build(endpoint.Destination, endpoint.RoutedParameters, configuration.Settings);
                    }
                }
            });
        }

        private static void AddRoute(Tuple<string, object, IDictionary<Type, Func<object, IDictionary<string, object>>>, string[]> routeToConfigure, IDictionary<string, object> environment)
        {
            var routeBuilder = environment.CreateRouteBuilder();

            routeBuilder.RestrictMethods(routeToConfigure.Item4);
            routeBuilder.AppendPattern(routeToConfigure.Item1);

            routeBuilder.Build(routeToConfigure.Item2, routeToConfigure.Item3, environment);
        }
    }
}