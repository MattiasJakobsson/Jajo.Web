using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenWeb.ModelBinding;

namespace OpenWeb.Endpoints
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebEndpointsMiddleware
    {
        private readonly AppFunc _next;
        private readonly IModelBinderCollection _modelBinderCollection;

        public OpenWebEndpointsMiddleware(AppFunc next, IModelBinderCollection modelBinderCollection)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _modelBinderCollection = modelBinderCollection;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = new OpenWebContext(environment, _modelBinderCollection);

            var executor = context.RoutedTo.GetCorrectEndpointExecutor(context);

            if (executor != null)
                await executor.Execute(context.RoutedTo, context);

            await _next(environment);
        }
    }
}