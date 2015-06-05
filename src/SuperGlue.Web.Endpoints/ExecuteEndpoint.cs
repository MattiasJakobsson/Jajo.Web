using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Endpoints
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteEndpoint
    {
        private readonly AppFunc _next;

        public ExecuteEndpoint(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routeInformation = environment.GetRouteInformation();

            var endpointExecutor = environment.Resolve<IExecuteAnyEndpoint>();

            if (routeInformation.RoutedTo != null)
                await endpointExecutor.Execute(environment, routeInformation.RoutedTo);

            await _next(environment);
        }
    }
}