using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public class ProjectionEventMappingContext<TState>
    {
        private readonly IDictionary<Type, Tuple<Action<object, EventContext<TState>>, Func<object, string>>> _eventHandlerMappings;

        public ProjectionEventMappingContext(IDictionary<Type, Tuple<Action<object, EventContext<TState>>, Func<object, string>>> eventHandlerMappings)
        {
            _eventHandlerMappings = eventHandlerMappings;
        }

        public void MapEventTo<TEvent>(Action<TEvent, EventContext<TState>> onArrived, Func<TEvent, string> findId) where TEvent : class
        {
            _eventHandlerMappings[typeof(TEvent)] = new Tuple<Action<object, EventContext<TState>>, Func<object, string>>((x, y) => onArrived((TEvent)x, y), x => findId((TEvent)x));
        }
    }
}