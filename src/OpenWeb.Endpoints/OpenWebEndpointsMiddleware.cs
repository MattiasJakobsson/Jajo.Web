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
            var routeInformation = environment.GetRouteInformation();

            if (routeInformation.RoutedTo != null)
            {
                var executor = environment.Resolve(typeof(IExecuteTypeOfEndpoint<>).MakeGenericType(routeInformation.RoutedTo.GetType()));

                if (executor != null)
                {
                    await (Task)executor
                        .GetType()
                        .GetMethod("Execute", new[] { routeInformation.RoutedTo.GetType(), typeof(IDictionary<string, object>) })
                        .Invoke(executor, new[] { routeInformation.RoutedTo, environment });

                    if(environment.GetOutput() == null)
                        environment.SetOutput("");
                }
            }

            await _next(environment);
        }
    }
}