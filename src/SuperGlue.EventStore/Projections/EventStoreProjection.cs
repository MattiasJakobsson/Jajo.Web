using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SuperGlue.EventStore.Projections
{
    public abstract class EventStoreProjection<TState> : IEventStoreProjection where TState : class, IProjectionState
    {
        private readonly IDictionary<string, TState> _instances = new Dictionary<string, TState>();
        private readonly ICollection<TState> _markedForDeletion = new Collection<TState>();
        private IDictionary<Type, Tuple<Action<object, EventContext<TState>>, Func<object, string>>> _eventHandlerMappings;
        private Func<string, TState> _createDefaultInstance;

        public virtual string ProjectionName { get { return string.Join("-", GetType().FullName.Split('.')).ToLower(); } }

        public abstract IEnumerable<string> GetInterestingStreams();

        public void Start()
        {
            var stateApplyer = GetStateApplyer();
            _eventHandlerMappings = stateApplyer.GetMappings();
            _createDefaultInstance = stateApplyer.Initialize;

            OnStarted();
        }

        public void Apply(object evnt, int version, IDictionary<string, object> metaData)
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
                    var projectionInstance = Load(id);

                    _instances[id] = projectionInstance;
                }

                handlerMapping.Item1(evnt, new EventContext<TState>(_instances[id], MarkForDeletion, metaData));

                _instances[id].SetVersion(version);
            }
        }

        public void Commit()
        {
            foreach (var instance in _instances)
                Commit(instance.Value);

            foreach (var instance in _markedForDeletion)
                Delete(instance);

            _instances.Clear();
            _markedForDeletion.Clear();
            _eventHandlerMappings.Clear();
            _createDefaultInstance = null;

            OnCommitted();
        }

        protected abstract void Commit(TState instance);
        protected abstract void Delete(TState instance);
        protected abstract TState Load(string id);
        protected abstract IApplyStateChangesTo<TState> GetStateApplyer();

        protected virtual void OnStarted() { }
        protected virtual void OnCommitted() { }

        protected virtual TState CreateDefaultInstance(string id)
        {
            return _createDefaultInstance(id);
        }

        protected void MarkForDeletion(TState instance)
        {
            if (!_markedForDeletion.Contains(instance))
                _markedForDeletion.Add(instance);
        }

        private IEnumerable<Type> GetTypesFrom(object evnt)
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
    }
}