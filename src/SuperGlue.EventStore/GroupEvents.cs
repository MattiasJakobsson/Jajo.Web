using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.EventStore
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class GroupEvents
    {
        private readonly AppFunc _next;
        private readonly GroupEventsOptions _options;

        public GroupEvents(AppFunc next, GroupEventsOptions options)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var groupedEvents = environment
                .GetEventStoreRequest()
                .Events
                .GroupBy(_options.GroupBy)
                .Where(x => x.Any());

            foreach (var groupedEvent in groupedEvents)
            {
                var childEnvironment = new Dictionary<string, object>();

                foreach (var item in environment)
                    childEnvironment[item.Key] = item.Value;

                childEnvironment.GetEventStoreRequest().Events = groupedEvent.ToList();
                childEnvironment[EventStoreEnvironmentExtensions.EventStoreConstants.EventsGroupedBy] = groupedEvent.Key;

                await _next(childEnvironment).ConfigureAwait(false);
            }
        }
    }

    public class GroupEventsOptions
    {
        public GroupEventsOptions(Func<DeSerializationResult, object> groupBy)
        {
            GroupBy = groupBy;
        }

        public Func<DeSerializationResult, object> GroupBy { get; private set; }
    }
}