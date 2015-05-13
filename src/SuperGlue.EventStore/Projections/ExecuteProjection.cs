using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Web;

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
            var projection = environment.Get<IEventStoreProjection>("superglue.EventStore.Projection");
            var events = environment.Get<IEnumerable<DeSerializationResult>>("superglue.EventStore.Events");
            var onError = environment.Get<Action<Exception, DeSerializationResult>>("superglue.EventStore.OnException");

            foreach (var evnt in events)
            {
                Exception lastException = null;
                var retryCount = 0;

                while (retryCount < 5)
                {
                    try
                    {
                        projection.Apply(evnt.Data, evnt.EventNumber, evnt.Metadata);
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

            await _next(environment);
        }
    }
}