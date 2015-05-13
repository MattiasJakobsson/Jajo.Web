using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Web;

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
            var processManager = environment.Get<IManageProcess>("superglue.EventStore.ProcessManager");
            var events = environment.Get<IEnumerable<DeSerializationResult>>("superglue.EventStore.Events");
            var onError = environment.Get<Action<Exception, DeSerializationResult>>("superglue.EventStore.OnException");

            processManager.Start();

            foreach (var evnt in events)
            {
                Exception lastException = null;
                var retryCount = 0;

                while (retryCount < 5)
                {
                    try
                    {
                        processManager.Apply(evnt.Data, evnt.EventNumber, evnt.Metadata);
                        lastException = null;
                        break;
                    }
                    catch (Exception exception)
                    {
                        lastException = exception;
                        retryCount++;
                    }
                }

                if (lastException != null)
                    onError(lastException, evnt);
            }

            processManager.Commit();

            await _next(environment);
        }
    }
}