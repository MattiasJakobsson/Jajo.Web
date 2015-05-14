using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SuperGlue
{
    internal static class RouteExtensions
    {
        public static RoutingData GetRouteInformation(this IDictionary<string, object> environment)
        {
            return new RoutingData(new ReadOnlyDictionary<string, object>(environment.Get<IDictionary<string, object>>("route.Parameters")), environment.Get<object>("route.RoutedTo"));
        }

        public static string RouteTo(this IDictionary<string, object> environment, object input)
        {
            var reverseRoute = environment.Get<Func<object, IDictionary<string, object>, string>>("superglue.ReverseRoute");

            if (reverseRoute == null)
                return "";

            var inputToRoute = environment.Get<IDictionary<Type, MethodInfo>>("superglue.RoutedEndpoints.Inputs");

            if (inputToRoute == null || !inputToRoute.ContainsKey(input.GetType()))
                return "";

            var inputParameters = environment.Get<Func<object, IDictionary<string, object>>>("superglue.RoutedEnpoints.ParametersFromInput")(input);

            return reverseRoute(inputToRoute[input.GetType()], inputParameters);
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