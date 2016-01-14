using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public class EventStream : IEventStream
    {
        public EventStream(IEnumerable<Event> events)
        {
            Events = events;
        }

        public IEnumerable<Event> Events { get; }
    }
}