using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jajo.Web.Endpoints
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteSpecificEndpoint
    {
        private readonly AppFunc _next;
        private readonly ExecuteSpecificEndpointOptions _options;

        public ExecuteSpecificEndpoint(AppFunc next, ExecuteSpecificEndpointOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (options == null)
                throw new ArgumentNullException("options");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routedTo = _options.GetRoute(environment);

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
                        environment["jajo.Output"] = "";
                }
            }

            await _next(environment);
        }
    }

    public class ExecuteSpecificEndpointOptions
    {
        public ExecuteSpecificEndpointOptions(Func<IDictionary<string, object>, object> getRoute)
        {
            GetRoute = getRoute;
        }

        public Func<IDictionary<string, object>, object> GetRoute { get; private set; } 
    }
}