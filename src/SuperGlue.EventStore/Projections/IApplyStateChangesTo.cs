using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    public interface IApplyStateChangesTo<TState>
    {
        IDictionary<Type, Tuple<Func<object, EventContext<TState>, Task>, Func<object, string>>> GetMappings();
        TState Initialize(string id);
    }
}