using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.EventStore.Data;

namespace SuperGlue.EventStore.ProcessManagers
{
    public abstract class BaseProcessManager<TState> : IManageProcess where TState : IProcessManagerState
    {
        private readonly IRepository _repository;

        private readonly IDictionary<string, TState> _instances = new Dictionary<string, TState>();
        private IReadOnlyDictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>, string>> _eventMappings;

        protected BaseProcessManager(IRepository repository)
        {
            _repository = repository;
        }

        public virtual string ProcessName { get { return GetType().FullName.Replace(".", "-").ToLower(); } }

        public IEnumerable<string> GetStreamsToProcess()
        {
            yield return GetTimeOutsStreamName();

            foreach (var stream in GetInterestingStreams())
                yield return stream;
        }

        public IReadOnlyDictionary<Type, string> GetEventMappings()
        {
            var eventMappings = new Dictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>, string>>();
            var mappingContext = new ProcessManagerEventMappingContext<TState>(eventMappings);

            MapEvents(mappingContext);

            return new ReadOnlyDictionary<Type, string>(eventMappings.ToDictionary(x => x.Key, x => x.Value.Item3));
        }

        public void Start()
        {
            var eventMappings = new Dictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>, string>>();
            var mappingContext = new ProcessManagerEventMappingContext<TState>(eventMappings);

            MapEvents(mappingContext);

            _eventMappings = new ReadOnlyDictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>, string>>(eventMappings);

            OnStarted();
        }

        public async Task Apply(object evnt, int version, IDictionary<string, object> metaData)
        {
            foreach (var type in GetTypesFrom(evnt))
            {
                if (!_eventMappings.ContainsKey(type))
                    continue;

                var handlerMapping = _eventMappings[type];

                var id = handlerMapping.Item2(evnt);

                if (!_instances.ContainsKey(id))
                {
                    var processManagerInstance = await Load(id);

                    _instances[id] = processManagerInstance;
                }

                await handlerMapping.Item1(evnt, _instances[id], metaData);
            }
        }

        public async Task Commit(IDictionary<string, object> environment)
        {
            foreach (var instance in _instances)
                await Save(instance.Value, environment);

            _instances.Clear();
        }

        protected abstract IEnumerable<string> GetInterestingStreams();

        private async Task<TState> Load(string id)
        {
            var state = CreateDefaultState(id);

            var events = await _repository.LoadStream(GetStreamName(id));

            state.BuildFromHistory(new EventStream(events));

            return state;
        }

        private async Task Save(TState state, IDictionary<string, object> environment)
        {
            var streamName = GetStreamName(state.Id);
            var changes = state.GetUncommittedChanges();

            await _repository.SaveToStream(streamName, changes.Events, new ReadOnlyDictionary<string, object>(state.GetMetaData(environment)));

            state.ClearUncommittedChanges();
        }

        protected abstract void MapEvents(ProcessManagerEventMappingContext<TState> mappingContext);
        protected abstract TState CreateDefaultState(string id);

        protected virtual void OnStarted() { }
        protected virtual void OnCommitted() { }

        protected Task RequestTimeOut(object evnt, IDictionary<string, object> metaData, DateTime at)
        {
            return _repository.RequestTimeOut(GetTimeOutsStreamName(), evnt, new ReadOnlyDictionary<string, object>(metaData), at);
        }

        protected virtual string GetStreamName(string id)
        {
            return string.Format("ProcessManagers-{0}-{1}", ProcessName, id);
        }

        protected virtual string GetTimeOutsStreamName()
        {
            return string.Format("ProcessManagers-{0}-TimeOuts", ProcessName);
        }

        private static IEnumerable<Type> GetTypesFrom(object evnt)
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