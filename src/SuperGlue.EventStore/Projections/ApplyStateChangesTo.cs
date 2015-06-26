using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public abstract class ApplyStateChangesTo<TState> : IApplyStateChangesTo<TState> where TState : IProjectionState
    {
        public IDictionary<Type, Tuple<Action<object, EventContext<TState>>, Func<object, string>>> GetMappings()
        {
            var eventHandlerMappings = new Dictionary<Type, Tuple<Action<object, EventContext<TState>>, Func<object, string>>>();

            MapInterestingEvents(new ProjectionEventMappingContext<TState>(eventHandlerMappings));

            return eventHandlerMappings;
        }

        public abstract TState Initialize(string id);

        protected abstract void MapInterestingEvents(ProjectionEventMappingContext<TState> mappingContext);
    }
}