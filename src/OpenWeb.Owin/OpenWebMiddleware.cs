using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebMiddleware
    {
        private readonly AppFunc _next;
        private readonly IWebEngine _webEngine;

        public OpenWebMiddleware(AppFunc next, IWebEngine webEngine)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (webEngine == null)
                throw new ArgumentNullException("webEngine");

            _next = next;
            _webEngine = webEngine;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var request = new Request(environment);

            await _webEngine.ExecuteRequest(request);

            await _next(environment);
        }
    }
}