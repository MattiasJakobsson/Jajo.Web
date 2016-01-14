using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.EventStore.Data;

namespace SuperGlue.EventStore.ProcessManagers
{
    public abstract class BaseProcessManager<TState> : IManageProcess where TState : IProcessManagerState, new()
    {
        private readonly IRepository _repository;
        
        protected BaseProcessManager(IRepository repository)
        {
            _repository = repository;
        }

        public virtual string ProcessName => GetType().FullName.Replace(".", "-").ToLower();

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

        public async Task Apply(object evnt, IDictionary<string, object> metaData)
        {
            var eventMappings = new Dictionary<Type, Tuple<Func<object, TState, IDictionary<string, object>, Task>, Func<object, string>, string>>();
            var mappingContext = new ProcessManagerEventMappingContext<TState>(eventMappings);

            MapEvents(mappingContext);

            foreach (var type in GetTypesFrom(evnt))
            {
                if (!eventMappings.ContainsKey(type))
                    continue;

                var handlerMapping = eventMappings[type];

                var id = handlerMapping.Item2(evnt);

                await handlerMapping.Item1(evnt, await _repository.Load<TState>(GetStreamName(id), id).ConfigureAwait(false), metaData).ConfigureAwait(false);
            }
        }

        protected abstract IEnumerable<string> GetInterestingStreams();

        protected abstract void MapEvents(ProcessManagerEventMappingContext<TState> mappingContext);

        protected Task RequestTimeOut(object evnt, DateTime at)
        {
            return _repository.RequestTimeOut(GetTimeOutsStreamName(), evnt, at);
        }

        protected Task RequestTimeOut(object evnt, TimeSpan @in)
        {
            return _repository.RequestTimeOut(GetTimeOutsStreamName(), evnt, @in);
        }

        protected virtual string GetStreamName(string id)
        {
            return $"ProcessManagers-{ProcessName}-{id}";
        }

        protected virtual string GetTimeOutsStreamName()
        {
            return $"ProcessManagers-{ProcessName}-TimeOuts";
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