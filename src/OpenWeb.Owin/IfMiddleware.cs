using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class IfMiddleware
    {
        private readonly AppFunc _next;
        private readonly IfOptions _options;

        public IfMiddleware(AppFunc next, IfOptions options)
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
            if (_options.Check(environment))
                await _options.Branch(environment);
            else
                await _next(environment);
        }
    }

    public class IfOptions
    {
        public IfOptions(Func<IDictionary<string, object>, bool> check)
        {
            Check = check;
        }

        public Func<IDictionary<string, object>, bool> Check { get; private set; }
        public AppFunc Branch { get; set; }
    }
}