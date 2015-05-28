using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SuperGlue
{
    internal static class RouteExtensions
    {
        public static class RouteConstants
        {
            public const string Parameters = "route.Parameters";
            public const string RoutedTo = "route.RoutedTo";
            public const string ReverseRoute = "superglue.ReverseRoute";
            public const string CreateRouteFunc = "superglue.CreateRouteFunc";
        }

        public static RoutingData GetRouteInformation(this IDictionary<string, object> environment)
        {
            return new RoutingData(new ReadOnlyDictionary<string, object>(environment.Get<IDictionary<string, object>>(RouteConstants.Parameters)), environment.Get<object>(RouteConstants.RoutedTo));
        }

        public static string RouteTo(this IDictionary<string, object> environment, object input)
        {
            var reverseRoute = environment.Get<Func<object, string>>(RouteConstants.ReverseRoute);

            return reverseRoute == null ? "" : reverseRoute(input);
        }

        public static void SetRouteDestination(this IDictionary<string, object> environment, object destination, IDictionary<string, object> parameters = null)
        {
            environment[RouteConstants.RoutedTo] = destination;
            environment[RouteConstants.Parameters] = parameters ?? new Dictionary<string, object>();
        }

        public static void CreateRoute(this IDictionary<string, object> environment, string pattern, object routeTo, params string[] methods)
        {
            environment.Get<Action<string, object, string[]>>(RouteConstants.CreateRouteFunc)(pattern, routeTo, methods);
        }

        public class RoutingData
        {
            public RoutingData(IReadOnlyDictionary<string, object> parameters, object routedTo)
            {
                Parameters = parameters;
                RoutedTo = routedTo;
            }

            public IReadOnlyDictionary<string, object> Parameters { get; private set; }
            public object RoutedTo { get; private set; }
        }
    }
}