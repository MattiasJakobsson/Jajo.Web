using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Continue
    {
        private readonly AppFunc _next;
        private readonly ContinueOptions _options;

        public Continue(AppFunc next, ContinueOptions options)
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
            _options.ContinueChain(environment);

            await _next(environment);
        }
    }

    public class ContinueOptions
    {
        public ContinueOptions(Action<IDictionary<string, object>> continueChain)
        {
            ContinueChain = continueChain;
        }

        public Action<IDictionary<string, object>>  ContinueChain { get; private set; } 
    }
}