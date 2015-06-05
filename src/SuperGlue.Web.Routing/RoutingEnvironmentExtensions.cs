using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Routing
{
    public static class RoutingEnvironmentExtensions
    {
        public static class RouteConstants
        {
            public const string Parameters = "route.Parameters";
            public const string RoutedTo = "route.RoutedTo";
            public const string ReverseRoute = "superglue.ReverseRoute";
            public const string CreateRouteFunc = "superglue.CreateRouteFunc";
            public const string CreateRouteBuilderFunc = "superglue.CreateRouteBuilderFunc";
        }

        public static IRouteBuilder CreateRouteBuilder(this IDictionary<string, object> environment)
        {
            return environment.Get<Func<IRouteBuilder>>(RouteConstants.CreateRouteBuilderFunc, () => null)();
        }
    }
}