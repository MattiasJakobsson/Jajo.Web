using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public interface IApplyStateChangesTo<TState>
    {
        IDictionary<Type, Tuple<Action<object, EventContext<TState>>, Func<object, string>>> GetMappings();
        TState Initialize(string id);
    }
}