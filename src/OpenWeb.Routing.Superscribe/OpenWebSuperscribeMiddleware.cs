using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Routing.Superscribe
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebSuperscribeMiddleware
    {
        private readonly AppFunc _next;

        public OpenWebSuperscribeMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);
        }
    }
}