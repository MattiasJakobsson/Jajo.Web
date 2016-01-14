using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public interface IEventStream
    {
        IEnumerable<Event> Events { get; }
    }
}