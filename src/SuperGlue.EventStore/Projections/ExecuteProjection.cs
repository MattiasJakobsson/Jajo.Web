using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteProjection
    {
        private readonly AppFunc _next;

        public ExecuteProjection(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var projection = environment.GetEventStoreRequest().Projection;
            var events = environment.GetEventStoreRequest().Events;
            var onError = environment.GetEventStoreRequest().OnException;

            foreach (var evnt in events)
            {
                Exception lastException = null;
                var retryCount = 0;

                while (retryCount < 5)
                {
                    try
                    {
                        await projection.Apply(evnt.Data, evnt.EventNumber, evnt.Metadata);
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

            await projection.Commit();

            await _next(environment);
        }
    }
}