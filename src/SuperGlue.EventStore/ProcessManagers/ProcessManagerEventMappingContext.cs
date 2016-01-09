using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ProcessManagerEventMappingContext<TState> where TState : IProcessManagerState
    {
        private readonly IDictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>, string>> _eventHandlerMappings;

        public ProcessManagerEventMappingContext(IDictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>, string>> eventHandlerMappings)
        {
            _eventHandlerMappings = eventHandlerMappings;
        }

        public void MapEventTo<TEvent>(Func<TEvent, TState, IDictionary<string, object>, Task> onArrived, Expression<Func<TEvent, string>> findId) where TEvent : class
        {
            var member = "";

            var memberExpression = findId.Body as MemberExpression;

            if (memberExpression != null)
                member = memberExpression.Member.Name;

            var findIdFunc = findId.Compile();

            _eventHandlerMappings[typeof(TEvent)] = new Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>, string>((x, y, z) => onArrived((TEvent)x, y, z), x => findIdFunc((TEvent)x), member);
        }
    }
}