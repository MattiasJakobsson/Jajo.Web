using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
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
                throw new ArgumentNullException(nameof(next));

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

            await environment.PushDiagnosticsData(DiagnosticsCategories.RequestsFor(environment), DiagnosticsTypes.RequestExecution, environment.GetCurrentChain().RequestId, new Tuple<string, IDictionary<string, object>>("RequestRouted", new Dictionary<string, object>
            {
                {"RoutedTo", data.Response ?? ""},
                {"Url", environment.GetRequest().Uri},
                {"Found", data.Response != null}
            })).ConfigureAwait(false);

            environment.SetRouteDestination(data.Response, endpoint != null ? endpoint.InputTypes : new List<Type>(), (IDictionary<string, object>)data.Parameters);

            await _next(environment).ConfigureAwait(false);
        }
    }
}