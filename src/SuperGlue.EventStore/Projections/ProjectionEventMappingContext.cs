using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    public class ProjectionEventMappingContext<TState>
    {
        private readonly IDictionary<Type, Tuple<Func<object, EventContext<TState>, Task>, Func<object, string>>> _eventHandlerMappings;

        public ProjectionEventMappingContext(IDictionary<Type, Tuple<Func<object, EventContext<TState>, Task>, Func<object, string>>> eventHandlerMappings)
        {
            _eventHandlerMappings = eventHandlerMappings;
        }

        public void MapEventTo<TEvent>(Func<TEvent, EventContext<TState>, Task> onArrived, Func<TEvent, string> findId) where TEvent : class
        {
            _eventHandlerMappings[typeof(TEvent)] = new Tuple<Func<object, EventContext<TState>, Task>, Func<object, string>>((x, y) => onArrived((TEvent)x, y), x => findId((TEvent)x));
        }
    }
}