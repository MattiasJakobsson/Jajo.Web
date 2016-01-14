using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.EventStore.Data;

namespace SuperGlue.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteProjection
    {
        private readonly AppFunc _next;

        public ExecuteProjection(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var projection = environment.GetEventStoreRequest().Projection;
            var events = environment.GetEventStoreRequest().Events;

            using (var stateApplier = projection.GetStateApplyer(environment))
            {
                foreach (var evnt in events)
                {
                    var correlationId = evnt.Metadata.Get(DefaultRepository.CorrelationIdKey, Guid.NewGuid().ToString());

                    using (environment.OpenCorrelationContext(correlationId))
                    using (environment.OpenCausationContext(evnt.EventId.ToString()))
                    {
                        await stateApplier.Apply(evnt.Data, evnt.EventNumber, evnt.Metadata);
                    }
                }
            }

            await _next(environment);
        }
    }
}