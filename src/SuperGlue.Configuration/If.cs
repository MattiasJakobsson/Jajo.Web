using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class If
    {
        private readonly AppFunc _next;
        private readonly IfOptions _options;

        public If(AppFunc next, IfOptions options)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var ifResult = _options.Check(environment);

            if (ifResult)
                await _options.Chain(environment).ConfigureAwait(false);

            if (_options.ShouldContinueWithNext(environment, ifResult))
                await _next(environment).ConfigureAwait(false);
        }
    }

    public class IfOptions
    {
        public IfOptions(Func<IDictionary<string, object>, bool> check, AppFunc chain, Func<IDictionary<string, object>, bool, bool> shouldContinueWithNext)
        {
            Check = check;
            Chain = chain;
            ShouldContinueWithNext = shouldContinueWithNext;
        }

        public Func<IDictionary<string, object>, bool> Check { get; }
        public AppFunc Chain { get; }
        public Func<IDictionary<string, object>, bool,  bool> ShouldContinueWithNext { get; }
    }
}