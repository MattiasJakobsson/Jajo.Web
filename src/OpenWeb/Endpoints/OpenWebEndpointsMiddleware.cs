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
            var context = new OpenWebContext(environment);

            var executor = context.RoutedTo.GetCorrectEndpointExecutor(context);

            if (executor != null)
                await executor.Execute(context.RoutedTo, context);

            await _next(environment);
        }
    }
}