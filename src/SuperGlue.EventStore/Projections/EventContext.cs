using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public class EventContext<TState>
    {
        private readonly Action<TState> _markForDeletion;

        public EventContext(TState state, Action<TState> markForDeletion, IDictionary<string, object> metaData)
        {
            _markForDeletion = markForDeletion;
            MetaData = metaData;
            State = state;
        }

        public TState State { get; private set; }
        public IDictionary<string, object> MetaData { get; private set; }

        public void MarkForDeletion()
        {
            _markForDeletion(State);
        }
    }
}