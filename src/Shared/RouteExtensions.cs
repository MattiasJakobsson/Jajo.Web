using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SuperGlue
{
    internal static class RouteExtensions
    {
        public static class RouteConstants
        {
            public const string Parameters = "route.Parameters";
            public const string RoutedTo = "route.RoutedTo";
            public const string ReverseRoute = "superglue.ReverseRoute";
            public const string EndpointInputs = "superglue.RoutedEndpoints.Inputs";
            public const string EndpointParametersFromInput = "superglue.RoutedEnpoints.ParametersFromInput";
        }

        public static RoutingData GetRouteInformation(this IDictionary<string, object> environment)
        {
            return new RoutingData(new ReadOnlyDictionary<string, object>(environment.Get<IDictionary<string, object>>(RouteConstants.Parameters)), environment.Get<object>(RouteConstants.RoutedTo));
        }

        public static string RouteTo(this IDictionary<string, object> environment, object input)
        {
            var reverseRoute = environment.Get<Func<object, IDictionary<string, object>, string>>(RouteConstants.ReverseRoute);

            if (reverseRoute == null)
                return "";

            var inputToRoute = environment.Get<IDictionary<Type, MethodInfo>>(RouteConstants.EndpointInputs);

            if (inputToRoute == null || !inputToRoute.ContainsKey(input.GetType()))
                return "";

            var inputParameters = environment.Get<Func<object, IDictionary<string, object>>>(RouteConstants.EndpointParametersFromInput)(input);

            return reverseRoute(inputToRoute[input.GetType()], inputParameters);
        }

        public static void SetRouteDestination(this IDictionary<string, object> environment, object destination, IDictionary<string, object> parameters = null)
        {
            environment[RouteConstants.RoutedTo] = destination;
            environment[RouteConstants.Parameters] = parameters ?? new Dictionary<string, object>();
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