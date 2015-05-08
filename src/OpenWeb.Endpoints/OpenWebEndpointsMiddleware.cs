using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebEndpointsMiddleware
    {
        private readonly AppFunc _next;

        public OpenWebEndpointsMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var executor = environment.GetRouteInformation().RoutedTo.GetCorrectEndpointExecutor(environment);

            if (executor != null)
                await executor.Execute(environment.GetRouteInformation().RoutedTo, environment);

            await _next(environment);
        }
    }
}