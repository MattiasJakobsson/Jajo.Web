using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Web;

namespace SuperGlue.EventStore.Subscribers
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
            var events = environment.Get<IEnumerable<DeSerializationResult>>("superglue.EventStore.Events").ToList();
            var subscriber = environment.Get<string>("superglue.EventStore.Subscriber");
            var stream = environment.Get<string>("superglue.EventStore.Stream");
            var manageStreamEventNumbers = environment.Resolve<IManageEventNumbersForSubscriber>();

            if (events.Any())
            {
                var lastEvent = events.Select(x => x.OriginalEventNumber).OrderByDescending(x => x).First();

                manageStreamEventNumbers.UpdateLastEvent(subscriber, stream, lastEvent);
            }

            await _next(environment);
        }
    }
}