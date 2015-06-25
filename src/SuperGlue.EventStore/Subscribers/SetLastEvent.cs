using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var events = environment.GetEventStoreRequest().Events.ToList();
            var subscriber = environment.GetEventStoreRequest().Service;
            var stream = environment.GetEventStoreRequest().Stream;
            var manageStreamEventNumbers = environment.Resolve<IManageEventNumbersForSubscriber>();

            if (events.Any())
            {
                var lastEvent = events.Select(x => x.OriginalEventNumber).OrderByDescending(x => x).First();

                await manageStreamEventNumbers.UpdateLastEvent(subscriber, stream, lastEvent);
            }

            await _next(environment);
        }
    }
}