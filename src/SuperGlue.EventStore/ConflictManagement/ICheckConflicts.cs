using System.Collections.Generic;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.ConflictManagement
{
    public interface ICheckConflicts
    {
        bool HasConflicts(IEnumerable<object> newEvents, IEnumerable<ResolvedEvent> storedEvents, IDictionary<string, object> environment);
    }
}