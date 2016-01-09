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
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var processManager = environment.GetEventStoreRequest().ProcessManager;
            var evnt = environment.GetEventStoreRequest().Event;

            processManager.Start();

            await processManager.Apply(evnt.Data, evnt.EventNumber, evnt.Metadata);

            await processManager.Commit(environment);

            await _next(environment);
        }
    }
}