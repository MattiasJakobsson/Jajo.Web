using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteEndpointMiddleware
    {
        private readonly AppFunc _next;
        private readonly Func<IDictionary<string, object>, object> _getRoute;

        public ExecuteEndpointMiddleware(AppFunc next, Func<IDictionary<string, object>, object> getRoute)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (getRoute == null)
                throw new ArgumentNullException("getRoute");

            _next = next;
            _getRoute = getRoute;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routedTo = _getRoute(environment);

            if (routedTo != null)
            {
                var executor = environment.Resolve(typeof(IExecuteTypeOfEndpoint<>).MakeGenericType(routedTo.GetType()));

                if (executor != null)
                {
                    await (Task)executor
                        .GetType()
                        .GetMethod("Execute", new[] { routedTo.GetType(), typeof(IDictionary<string, object>) })
                        .Invoke(executor, new[] { routedTo, environment });

                    if (environment.GetOutput() == null)
                        environment.SetOutput("");
                }
            }

            await _next(environment);
        }
    }
}