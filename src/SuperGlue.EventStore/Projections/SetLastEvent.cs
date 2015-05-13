using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Web;

namespace SuperGlue.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SetLastEvent
    {
        private readonly AppFunc _next;

        public SetLastEvent(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var projection = environment.Get<IEventStoreProjection>("superglue.EventStore.Projection");
            var events = environment.Get<IEnumerable<DeSerializationResult>>("superglue.EventStore.Events").ToList();
            var eventNumbersForProjections = environment.Resolve<IManageEventNumbersForProjections>();

            if (events.Any())
            {
                var lastEvent = events.Select(x => x.OriginalEventNumber).OrderByDescending(x => x).First();

                eventNumbersForProjections.UpdateLastEvent(projection.ProjectionName, lastEvent);
            }

            await _next(environment);
        }
    }
}