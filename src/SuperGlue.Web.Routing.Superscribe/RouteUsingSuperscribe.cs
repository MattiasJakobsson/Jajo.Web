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

        public RouteUsingSuperscribe(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var path = environment.GetRequest().Path + "?" + environment.GetRequest().QueryString;
            var method = environment.GetRequest().Method;

            var routeEngine = environment.GetRouteEngine();

            var routeData = new RouteData { Environment = environment };
            var walker = routeEngine.Walker();
            var data = walker.WalkRoute(path, method, routeData);

            var endpoint = environment.GetRouteForEndpoint(data.Response);

            if (endpoint == null)
                environment.PushDiagnosticsData(DiagnosticTypes.MiddleWareExecutionFor(environment), new Tuple<string, IDictionary<string, object>>("MissedRoute", new Dictionary<string, object>()));
            else
                environment.PushDiagnosticsData(DiagnosticTypes.MiddleWareExecutionFor(environment), new Tuple<string, IDictionary<string, object>>("FoundRoute", new Dictionary<string, object>
                {
                    {"RoutedTo", endpoint},
                    {"Url", environment.GetRequest().Uri.ToString()}
                }));

            environment.SetRouteDestination(data.Response, endpoint != null ? endpoint.InputTypes : new List<Type>(), (IDictionary<string, object>)data.Parameters);

            await _next(environment);
        }
    }
}