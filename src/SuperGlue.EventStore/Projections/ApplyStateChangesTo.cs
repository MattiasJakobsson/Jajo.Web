using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    public abstract class ApplyStateChangesTo<TState> : IApplyStateChangesTo
    {
        private readonly IDictionary<string, TState> _instances = new Dictionary<string, TState>();
        private readonly ICollection<TState> _markedForDeletion = new Collection<TState>();
        private readonly IDictionary<Type, Tuple<Func<object, EventContext<TState>, Task>, Func<object, string>>> _eventHandlerMappings;

        protected ApplyStateChangesTo()
        {
            var eventHandlerMappings = new Dictionary<Type, Tuple<Func<object, EventContext<TState>, Task>, Func<object, string>>>();

            MapInterestingEvents(new ProjectionEventMappingContext<TState>(eventHandlerMappings));

            _eventHandlerMappings = eventHandlerMappings;
        }

        public async Task Apply(object evnt, int version, IDictionary<string, object> metaData)
        {
            foreach (var type in GetTypesFrom(evnt))
            {
                if (!_eventHandlerMappings.ContainsKey(type))
                    continue;

                var handlerMapping = _eventHandlerMappings[type];

                var id = handlerMapping.Item2(evnt);

                if (string.IsNullOrEmpty(id))
                    continue;

                if (!_instances.ContainsKey(id))
                {
                    var projectionInstance = await Load(id);

                    _instances[id] = projectionInstance;
                }

                await handlerMapping.Item1(evnt, new EventContext<TState>(_instances[id], metaData));
            }
        }

        public void Dispose()
        {
            foreach (var instance in _markedForDeletion)
                Delete(instance);

            _instances.Clear();
            _markedForDeletion.Clear();
            _eventHandlerMappings.Clear();
        }

        protected abstract Task<TState> Load(string id);

        protected abstract void Delete(TState instance);

        protected abstract void MapInterestingEvents(ProjectionEventMappingContext<TState> mappingContext);

        protected virtual IEnumerable<Type> GetTypesFrom(object evnt)
        {
            if (evnt == null)
                yield break;

            yield return evnt.GetType();

            var baseType = evnt.GetType().BaseType;

            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }

            foreach (var @interface in evnt.GetType().GetInterfaces())
                yield return @interface;
        }

        protected void MarkForDeletion(TState instance)
        {
            if (!_markedForDeletion.Contains(instance))
                _markedForDeletion.Add(instance);
        }
    }
}