using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    public abstract class ApplyStateChangesTo<TState> : IApplyStateChangesTo<TState>
    {
        public IDictionary<Type, Tuple<Func<object, EventContext<TState>, Task>, Func<object, string>>> GetMappings()
        {
            var eventHandlerMappings = new Dictionary<Type, Tuple<Func<object, EventContext<TState>, Task>, Func<object, string>>>();

            MapInterestingEvents(new ProjectionEventMappingContext<TState>(eventHandlerMappings));

            return eventHandlerMappings;
        }

        public abstract TState Initialize(string id);

        protected abstract void MapInterestingEvents(ProjectionEventMappingContext<TState> mappingContext);
    }
}