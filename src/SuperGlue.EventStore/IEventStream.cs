using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public interface IEventStream
    {
        IEnumerable<object> Events { get; }
    }
}