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

            using (var stateApplier = projection.GetStateApplyer(environment))
            {
                foreach (var evnt in events)
                {
                    await stateApplier.Apply(evnt.Data, evnt.EventNumber, evnt.Metadata);
                }
            }

            await _next(environment);
        }
    }
}