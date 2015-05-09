using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SetStatusCodeMiddleware
    {
        private readonly AppFunc _next;
        private readonly int _statusCode;

        public SetStatusCodeMiddleware(AppFunc next, int statusCode)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _statusCode = statusCode;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            environment.SetStatusCode(_statusCode);

            await _next(environment);
        }
    }
}