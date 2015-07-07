using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public class EventContext<TState>
    {
        public EventContext(TState state, IDictionary<string, object> metaData)
        {
            MetaData = metaData;
            State = state;
        }

        public TState State { get; private set; }
        public IDictionary<string, object> MetaData { get; private set; }
    }
}