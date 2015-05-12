using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Superscribe.Engine;

namespace Jajo.Web.Routing.Superscribe
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
            var path = environment["owin.RequestPath"].ToString();
            var method = environment["owin.RequestMethod"].ToString();

            var routeData = new RouteData { Environment = environment };
            var walker = _options.RouteEngine.Walker();
            var data = walker.WalkRoute(path, method, routeData);

            environment["route.RoutedTo"] = data.Response;
            environment["route.Parameters"] = data.Parameters;

            if (_options.ApplicationSettings.ContainsKey("jajo.ReverseRoute"))
                environment["jajo.ReverseRoute"] = _options.ApplicationSettings["jajo.ReverseRoute"];

            if (_options.ApplicationSettings.ContainsKey("jajo.RoutedEndpoints.Inputs"))
                environment["jajo.RoutedEndpoints.Inputs"] = _options.ApplicationSettings["jajo.RoutedEndpoints.Inputs"];

            if (_options.ApplicationSettings.ContainsKey("jajo.RoutedEnpoints.ParametersFromInput"))
                environment["jajo.RoutedEnpoints.ParametersFromInput"] = _options.ApplicationSettings["jajo.RoutedEnpoints.ParametersFromInput"];

            await _next(environment);
        }
    }

    public class RouteUsingSuperscribeOptions
    {
        public RouteUsingSuperscribeOptions(IRouteEngine routeEngine, IDictionary<string, object> applicationSettings)
        {
            RouteEngine = routeEngine;
            ApplicationSettings = applicationSettings;
        }

        public IRouteEngine RouteEngine { get; private set; }
        public IDictionary<string, object> ApplicationSettings { get; private set; }
    }
}