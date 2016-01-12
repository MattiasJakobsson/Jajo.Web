using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Monitoring
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InvokeHeartBeat
    {
        private readonly AppFunc _next;

        public InvokeHeartBeat(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var heartBeatMonitor = environment.GetHeartBeatMonitor();

            await heartBeatMonitor.Beat(environment);

            await _next(environment);
        }
    }
}