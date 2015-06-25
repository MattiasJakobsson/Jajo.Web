using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using SuperGlue.EventStore.ConflictManagement;
using SuperGlue.EventStore.Timeouts;

namespace SuperGlue.EventStore.Data
{
    public class Repository : IRepository
    {
        private readonly IInstantiateAggregate _instantiateAggregate;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly ICheckConflicts _checkConflicts;
        private readonly IHandleStreamNames _handleStreamNames;
        private readonly IEnumerable<IManageChanges> _manageChanges;
        private readonly IManageTimeOuts _timeoutManager;
        private readonly ConcurrentDictionary<string, LoadedAggregate> _loadedAggregates = new ConcurrentDictionary<string, LoadedAggregate>();

        private const string AggregateClrTypeHeader = "AggregateClrTypeName";
        private const string CommitIdHeader = "CommitId";
        private const string AggregateIdHeader = "AggregateId";
        private const string ContextHeader = "Context";
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;

        public Repository(IInstantiateAggregate instantiateAggregate, IEventStoreConnection eventStoreConnection, IHandleEventSerialization eventSerialization, ICheckConflicts checkConflicts, IHandleStreamNames handleStreamNames,
            IEnumerable<IManageChanges> manageChanges, IManageTimeOuts timeoutManager)
        {
            _instantiateAggregate = instantiateAggregate;
            _eventStoreConnection = eventStoreConnection;
            _eventSerialization = eventSerialization;
            _checkConflicts = checkConflicts;
            _handleStreamNames = handleStreamNames;
            _manageChanges = manageChanges;
            _timeoutManager = timeoutManager;
        }

        public async Task<T> Load<T>(string id, ActionMetaData actionMetaData) where T : IAggregate, new()
        {
            LoadedAggregate aggregate;

            if (_loadedAggregates.TryGetValue(id, out aggregate))
                return (T)aggregate.Aggregate;

            return await LoadVersion<T>(id, int.MaxValue, actionMetaData);
        }

        public async Task<T> LoadVersion<T>(string id, int version, ActionMetaData actionMetaData) where T : IAggregate, new()
        {
            actionMetaData = actionMetaData ?? new ActionMetaData(new Dictionary<string, object>());

            var aggregate = _instantiateAggregate.Instantiate<T>(id);
            var streamName = _handleStreamNames.GetAggregateStreamName(aggregate);

            var events = await LoadEventsFromStream(streamName, 0, version);

            aggregate.BuildFromHistory(new EventStream(events.Select(DeserializeEvent)));

            if (aggregate.Version != version && version < int.MaxValue)
                throw new AggregateVersionException(id, typeof(T), aggregate.Version, version);

            OnAggregateLoaded(aggregate, actionMetaData);

            return aggregate;
        }

        public async Task<IEnumerable<object>> LoadStream(string stream)
        {
            return (await LoadEventsFromStream(stream, 0, int.MaxValue)).Select(DeserializeEvent);
        }

        public async Task RequestTimeOut(string stream, Guid commitId, object evnt, IReadOnlyDictionary<string, object> metaData, DateTime at)
        {
            var commitHeaders = metaData.ToDictionary(x => x.Key, x => x.Value);
            commitHeaders[CommitIdHeader] = commitId;

            await _timeoutManager.RequestTimeOut(stream, commitId, evnt, at, commitHeaders);
        }

        public async Task SaveChanges()
        {
            foreach (var aggregate in _loadedAggregates)
                await Save(aggregate.Value.Aggregate, Guid.NewGuid(), aggregate.Value.MetaData);
        }

        private async Task Save(IAggregate aggregate, Guid commitId, ActionMetaData actionMetaData)
        {
            var commitHeaders = new Dictionary<string, object>();

            foreach (var item in actionMetaData.MetaData)
                commitHeaders[item.Key] = item.Value;

            commitHeaders[CommitIdHeader] = commitId;
            commitHeaders[AggregateClrTypeHeader] = aggregate.GetType().AssemblyQualifiedName;
            commitHeaders[ContextHeader] = aggregate.Context;
            commitHeaders[AggregateIdHeader] = aggregate.Id;

            var streamName = _handleStreamNames.GetAggregateStreamName(aggregate);
            var eventStream = aggregate.GetUncommittedChanges();
            var newEvents = eventStream.Events.ToList();
            var originalVersion = aggregate.Version - newEvents.Count;

            var versionToExpect = originalVersion == 0 ? ExpectedVersion.Any : originalVersion;

            if (actionMetaData.ExpectedVersion.HasValue)
                versionToExpect = actionMetaData.ExpectedVersion.Value;

            while (true)
            {
                try
                {
                    await SaveEventsToStream(streamName, versionToExpect, newEvents, commitHeaders);
                    break;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is WrongExpectedVersionException))
                        throw;
                }

                var storedEvents = (await LoadEventsFromStream(streamName, versionToExpect < 0 ? 0 : versionToExpect, int.MaxValue)).ToList();

                var currentVersion = storedEvents.Select(x => x.OriginalEventNumber).OrderByDescending(x => x).FirstOrDefault();

                if (_checkConflicts.HasConflicts(newEvents, storedEvents, actionMetaData.Environment))
                    throw new ConflictingEventException(streamName, versionToExpect, currentVersion);

                versionToExpect = currentVersion;
            }

            aggregate.ClearUncommittedChanges();
        }

        public async Task SaveToStream(string stream, IEnumerable<object> events, Guid commitId, ActionMetaData actionMetaData)
        {
            var commitHeaders = new Dictionary<string, object>();

            foreach (var item in actionMetaData.MetaData)
                commitHeaders[item.Key] = item.Value;

            commitHeaders[CommitIdHeader] = commitId;

            var newEvents = events.ToList();

            await SaveEventsToStream(stream, actionMetaData.ExpectedVersion ?? ExpectedVersion.Any, newEvents, commitHeaders);
        }

        public async Task SaveToStream(string stream, IEnumerable<object> events, Guid commitId, string context, ActionMetaData actionMetaData)
        {
            var commitHeaders = new Dictionary<string, object>();

            foreach (var item in actionMetaData.MetaData)
                commitHeaders[item.Key] = item.Value;

            commitHeaders[CommitIdHeader] = commitId;
            commitHeaders[ContextHeader] = context;

            var streamName = _handleStreamNames.GetStreamName(stream, context);
            var newEvents = events.ToList();

            await SaveEventsToStream(streamName, actionMetaData.ExpectedVersion ?? ExpectedVersion.Any, newEvents, commitHeaders);
        }

        public async Task SaveToNamedStream(string stream, IEnumerable<object> events, Guid commitId, string context, ActionMetaData actionMetaData)
        {
            var commitHeaders = new Dictionary<string, object>();

            foreach (var item in actionMetaData.MetaData)
                commitHeaders[item.Key] = item.Value;

            commitHeaders[CommitIdHeader] = commitId;
            commitHeaders[ContextHeader] = context;

            var newEvents = events.ToList();

            await SaveEventsToStream(stream, ExpectedVersion.Any, newEvents, commitHeaders);
        }

        public void Attache(IAggregate aggregate, ActionMetaData actionMetaData)
        {
            actionMetaData = actionMetaData ?? new ActionMetaData(new Dictionary<string, object>());

            OnAggregateLoaded(aggregate, actionMetaData);
        }

        public event Action<IAggregate, ActionMetaData> AggregateLoaded;

        protected async Task SaveEventsToStream(string streamName, int expectedVersion, IReadOnlyCollection<object> events, IDictionary<string, object> commitHeaders)
        {
            var eventsToSave = events.Select(e => ToEventData(Guid.NewGuid(), e, commitHeaders)).ToList();

            if (!eventsToSave.Any())
                return;

            if (eventsToSave.Count < WritePageSize)
            {
                await _eventStoreConnection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave);
            }
            else
            {
                var transaction = await _eventStoreConnection.StartTransactionAsync(streamName, expectedVersion);

                var position = 0;
                while (position < eventsToSave.Count)
                {
                    var pageEvents = eventsToSave.Skip(position).Take(WritePageSize);
                    await transaction.WriteAsync(pageEvents);
                    position += WritePageSize;
                }

                await transaction.CommitAsync();
            }

            foreach (var manageChanges in _manageChanges)
                manageChanges.ChangesSaved(events, commitHeaders);
        }

        protected async Task<IEnumerable<ResolvedEvent>> LoadEventsFromStream(string streamName, int from, int to)
        {
            var sliceStart = from;
            StreamEventsSlice currentSlice;
            var result = new List<ResolvedEvent>();

            do
            {
                var sliceCount = sliceStart + ReadPageSize <= to
                    ? ReadPageSize
                    : to - sliceStart;

                if (sliceCount == 0)
                    break;

                currentSlice = await _eventStoreConnection.ReadStreamEventsForwardAsync(streamName, sliceStart, sliceCount, false);

                if (currentSlice.Status == SliceReadStatus.StreamDeleted)
                    throw new StreamDeletedException(streamName);

                sliceStart = currentSlice.NextEventNumber;

                result.AddRange(currentSlice.Events);
            } while (to >= currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            return result;
        }

        protected void OnAggregateLoaded(IAggregate aggregate, ActionMetaData actionMetaData)
        {
            aggregate.AggregateAttached += x => OnAggregateLoaded(x, actionMetaData);

            _loadedAggregates[aggregate.Id] = new LoadedAggregate(aggregate, actionMetaData);

            var handler = AggregateLoaded;
            if (handler != null) handler(aggregate, actionMetaData);
        }

        private EventData ToEventData(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var serializedEvent = _eventSerialization.Serialize(eventId, evnt, headers);

            return new EventData(serializedEvent.EventId, serializedEvent.Type, serializedEvent.IsJson, serializedEvent.Data, serializedEvent.Metadata);
        }

        private object DeserializeEvent(ResolvedEvent evnt)
        {
            return _eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data).Data;
        }

        private class LoadedAggregate
        {
            public LoadedAggregate(IAggregate aggregate, ActionMetaData metaData)
            {
                Aggregate = aggregate;
                MetaData = metaData;
            }

            public IAggregate Aggregate { get; private set; }
            public ActionMetaData MetaData { get; private set; }
        }
    }
}