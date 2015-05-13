using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public interface IEventHandlerMappingStrategy
    {
        IEnumerable<IEventHandler> GetEventHandlers(object target);
    }
}