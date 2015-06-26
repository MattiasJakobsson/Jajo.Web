using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ProcessManagerEventMappingContext<TState> where TState : IProcessManagerState
    {
        private readonly IDictionary<Type, Tuple<Action<object, TState, IDictionary<string, object>>, Func<object, string>>> _eventHandlerMappings;

        public ProcessManagerEventMappingContext(IDictionary<Type, Tuple<Action<object, TState, IDictionary<string, object>>, Func<object, string>>> eventHandlerMappings)
        {
            _eventHandlerMappings = eventHandlerMappings;
        }

        public void MapEventTo<TEvent>(Action<TEvent, TState, IDictionary<string, object>> onArrived, Func<TEvent, string> findId) where TEvent : class
        {
            _eventHandlerMappings[typeof(TEvent)] = new Tuple<Action<object, TState, IDictionary<string, object>>, Func<object, string>>((x, y, z) => onArrived((TEvent)x, y, z), x => findId((TEvent)x));
        }
    }
}