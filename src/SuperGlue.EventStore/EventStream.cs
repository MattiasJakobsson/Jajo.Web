using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public class EventStream : IEventStream
    {
        public EventStream(IEnumerable<object> events)
        {
            Events = events;
        }

        public IEnumerable<object> Events { get; private set; }
    }
}