using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SetLastEvent
    {
        private readonly AppFunc _next;

        public SetLastEvent(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var projection = environment.GetEventStoreRequest().Projection;
            var events = environment.GetEventStoreRequest().Events.ToList();
            var eventNumbersForProjections = environment.Resolve<IManageEventNumbersForProjections>();

            if (events.Any())
            {
                var lastEvent = events.Select(x => x.OriginalEventNumber).OrderByDescending(x => x).First();

                await eventNumbersForProjections.UpdateLastEvent(projection.ProjectionName, lastEvent, environment).ConfigureAwait(false);
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}