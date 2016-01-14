using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.ProcessManagers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteProcessManager
    {
        private readonly AppFunc _next;

        public ExecuteProcessManager(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var processManager = environment.GetEventStoreRequest().ProcessManager;
            var evnt = environment.GetEventStoreRequest().Event;

            await processManager.Apply(evnt.Data, evnt.Metadata);

            await _next(environment);
        }
    }
}