using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Hosting.SystemWeb
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RunAppFunc
    {
        private readonly AppFunc _next;
        private readonly RunAppFuncOptions _options;

        public RunAppFunc(AppFunc next, RunAppFuncOptions options)
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
            await _options.Func(environment);
        }
    }

    public class RunAppFuncOptions
    {
        public RunAppFuncOptions(AppFunc func)
        {
            Func = func;
        }

        public AppFunc Func { get; private set; } 
    }
}