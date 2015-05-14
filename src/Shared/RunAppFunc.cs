using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Hosting
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
            foreach (var item in _options.ApplicationEnvironment.Where(item => !environment.ContainsKey(item.Key)))
                environment[item.Key] = item.Value;

            await _options.Func(environment);

            await _next(environment);
        }
    }

    public class RunAppFuncOptions
    {
        public RunAppFuncOptions(AppFunc func, IDictionary<string, object> applicationEnvironment)
        {
            Func = func;
            ApplicationEnvironment = applicationEnvironment;
        }

        public AppFunc Func { get; private set; }
        public IDictionary<string, object> ApplicationEnvironment { get; private set; }
    }
}