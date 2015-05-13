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
                        environment["superglue.Output"] = "";
                }
            }

            await _next(environment);
        }
    }
}