using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ProcessManagerEventMappingContext<TState> where TState : IProcessManagerState
    {
        private readonly IDictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>>> _eventHandlerMappings;

        public ProcessManagerEventMappingContext(IDictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>>> eventHandlerMappings)
        {
            _eventHandlerMappings = eventHandlerMappings;
        }

        public void MapEventTo<TEvent>(Func<TEvent, TState, IDictionary<string, object>, Task> onArrived, Func<TEvent, string> findId) where TEvent : class
        {
            _eventHandlerMappings[typeof(TEvent)] = new Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>>((x, y, z) => onArrived((TEvent)x, y, z), x => findId((TEvent)x));
        }
    }
}