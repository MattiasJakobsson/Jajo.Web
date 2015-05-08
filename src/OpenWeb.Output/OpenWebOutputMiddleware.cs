using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Output
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebOutputMiddleware
    {
        private readonly AppFunc _next;
        private readonly IHandleOutputRendering _handleOutputRendering;

        public OpenWebOutputMiddleware(AppFunc next, IHandleOutputRendering handleOutputRendering)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _handleOutputRendering = handleOutputRendering;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _handleOutputRendering.Render(environment);

            await _next(environment);
        }
    }
}