using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Jajo.Web.Diagnostics
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class MeasureInner
    {
        private readonly AppFunc _next;
        private readonly MeasureInnerOptions _options;

        public MeasureInner(AppFunc next, MeasureInnerOptions options)
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _next(environment);
        
            stopwatch.Stop();

            _options.WriteTo(stopwatch.Elapsed, environment);
        }
    }

    public class MeasureInnerOptions
    {
        public MeasureInnerOptions(Action<TimeSpan, IDictionary<string, object>> writeTo)
        {
            WriteTo = writeTo;
        }

        public Action<TimeSpan, IDictionary<string, object>> WriteTo { get; private set; } 
    }
}