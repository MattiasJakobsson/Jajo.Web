using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Superscribe.Engine;

namespace OpenWeb.Routing.Superscribe
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebSuperscribeMiddleware
    {
        private readonly AppFunc _next;
        private readonly IRouteEngine _routeEngine;

        public OpenWebSuperscribeMiddleware(AppFunc next, IRouteEngine routeEngine)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _routeEngine = routeEngine;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var path = environment["owin.RequestPath"].ToString();
            var method = environment["owin.RequestMethod"].ToString();

            var routeData = new RouteData { Environment = environment };
            var walker = _routeEngine.Walker();
            var data = walker.WalkRoute(path, method, routeData);

            var routeTo = data.Response as MethodInfo;

            if (routeTo != null)
                environment["route.RoutedTo"] = routeTo;

            environment["route.Parameters"] = data.Parameters;

            await _next(environment);
        }
    }
}