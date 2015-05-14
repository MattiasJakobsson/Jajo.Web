using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Superscribe.Engine;

namespace SuperGlue.Web.Routing.Superscribe
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RouteUsingSuperscribe
    {
        private readonly AppFunc _next;
        private readonly RouteUsingSuperscribeOptions _options;

        public RouteUsingSuperscribe(AppFunc next, RouteUsingSuperscribeOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var path = environment.GetRequest().Path;
            var method = environment.GetRequest().Method;

            var routeData = new RouteData { Environment = environment };
            var walker = _options.RouteEngine.Walker();
            var data = walker.WalkRoute(path, method, routeData);

            environment.SetRouteDestination(data.Response, (IDictionary<string, object>)data.Parameters);

            await _next(environment);
        }
    }

    public class RouteUsingSuperscribeOptions
    {
        public RouteUsingSuperscribeOptions(IRouteEngine routeEngine)
        {
            RouteEngine = routeEngine;
        }

        public IRouteEngine RouteEngine { get; private set; }
    }
}