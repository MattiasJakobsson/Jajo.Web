using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using SuperGlue.EventStore.ConflictManagement;
using SuperGlue.EventStore.Timeouts;
using SuperGlue.Logging;
using SuperGlue.MetaData;

namespace SuperGlue.EventStore.Data
{
    public class DefaultRepository : IRepository
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly ICheckConflicts _checkConflicts;
        private readonly IEnumerable<IManageChanges> _manageChanges;
        private readonly IManageTimeOuts _timeoutManager;
        private readonly ILog _log;
        private readonly IDictionary<string, object> _environment;
        private readonly ConcurrentDictionary<string, LoadedAggregate> _loadedAggregates = new ConcurrentDictionary<string, LoadedAggregate>();

        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;

        public DefaultRepository(IEventStoreConnection eventStoreConnection, IHandleEventSerialization eventSerialization, ICheckConflicts checkConflicts,
            IEnumerable<IManageChanges> manageChanges, IManageTimeOuts timeoutManager, IDictionary<string, object> environment, ILog log)
        {
            _eventStoreConnection = eventStoreConnection;
            _eventSerialization = eventSerialization;
            _checkConflicts = checkConflicts;
            _manageChanges = manageChanges;
            _timeoutManager = timeoutManager;
            _environment = environment;
            _log = log;
        }

        public async Task<T> Load<T>(string id) where T : IAggregate, new()
        {
            LoadedAggregate aggregate;

            if (_loadedAggregates.TryGetValue(id, out aggregate))
                return (T)aggregate.Aggregate;

            return await LoadVersion<T>(id, int.MaxValue);
        }

        public async Task<T> LoadVersion<T>(string id, int version) where T : IAggregate, new()
        {
            var aggregate = new T
            {
                Id = id
            };

            var streamName = aggregate.GetStreamName(_environment);

            var events = await LoadEventsFromStream(streamName, 0, version);

            aggregate.BuildFromHistory(new EventStream(events.Select(DeserializeEvent)));

            if (aggregate.Version != version && version < int.MaxValue)
                throw new AggregateVersionException(id, typeof(T), aggregate.Version, version);

            OnAggregateLoaded(aggregate);

            return aggregate;
        }

        public async Task<IEnumerable<object>> LoadStream(string stream)
        {
            return (await LoadEventsFromStream(stream, 0, int.MaxValue)).Select(DeserializeEvent);
        }

        public async Task RequestTimeOut(string stream, object evnt, IReadOnlyDictionary<string, object> metaData, DateTime at)
        {
            var commitHeaders = metaData.ToDictionary(x => x.Key, x => x.Value);

            await _timeoutManager.RequestTimeOut(stream, Guid.NewGuid(), evnt, at, commitHeaders);
        }

        public async Task SaveChanges()
        {
            foreach (var aggregate in _loadedAggregates)
                await Save(aggregate.Value.Aggregate);
        }

        private async Task Save(IAggregate aggregate)
        {
            var commitHeaders = new Dictionary<string, object>();

            var metaData = _environment.GetMetaData().MetaData;

            foreach (var item in metaData)
                commitHeaders[item.Key] = item.Value;

            var aggregateMetaData = aggregate.GetMetaData(_environment);

            foreach (var item in aggregateMetaData)
                commitHeaders[item.Key] = item.Value;

            var streamName = aggregate.GetStreamName(_environment);
            var eventStream = aggregate.GetUncommittedChanges();
            var newEvents = eventStream.Events.ToList();
            var originalVersion = aggregate.Version - newEvents.Count;

            var versionToExpect = originalVersion == 0 ? ExpectedVersion.Any : originalVersion - 1;

            while (true)
            {
                try
                {
                    await SaveEventsToStream(streamName, versionToExpect, newEvents, commitHeaders);
                    break;
                }
                catch (WrongExpectedVersionException ex)
                {
                    _log.Debug(ex, "Events where added to aggregate with id: {0} since last load. Checking for conflicts and trying again...", aggregate.Id);
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is WrongExpectedVersionException))
                        throw;

                    _log.Debug(ae.InnerException, "Events where added to aggregate with id: {0} since last load. Checking for conflicts and trying again...", aggregate.Id);
                }

                var storedEvents = (await LoadEventsFromStream(streamName, versionToExpect < 0 ? 0 : versionToExpect, int.MaxValue)).ToList();

                var currentVersion = storedEvents.Select(x => x.OriginalEventNumber).OrderByDescending(x => x).FirstOrDefault();

                if (_checkConflicts.HasConflicts(newEvents, storedEvents, _environment))
                    throw new ConflictingEventException(streamName, versionToExpect, currentVersion);

                versionToExpect = currentVersion;
            }

            aggregate.ClearUncommittedChanges();
        }

        public async Task SaveToStream(string stream, IEnumerable<object> events, IReadOnlyDictionary<string, object> metaData)
        {
            var commitHeaders = metaData.ToDictionary(x => x.Key, x => x.Value);

            var requestMetaData = _environment.GetMetaData().MetaData;

            foreach (var item in requestMetaData)
                commitHeaders[item.Key] = item.Value;

            var newEvents = events.ToList();

            await SaveEventsToStream(stream, ExpectedVersion.Any, newEvents, commitHeaders);
        }

        public void Attache(IAggregate aggregate)
        {
            OnAggregateLoaded(aggregate);
        }

        public event Action<IAggregate> AggregateLoaded;

        protected async Task SaveEventsToStream(string streamName, int expectedVersion, IReadOnlyCollection<object> events, IDictionary<string, object> commitHeaders)
        {
            var eventsToSave = events.Select(e => ToEventData(Guid.NewGuid(), e, commitHeaders)).ToList();

            if (!eventsToSave.Any())
                return;

            if (eventsToSave.Count < WritePageSize)
            {
                await _eventStoreConnection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave);
                _log.Debug("Saved {0} events to stream {1}.", eventsToSave.Count, streamName);
            }
            else
            {
                var transaction = await _eventStoreConnection.StartTransactionAsync(streamName, expectedVersion);

                var position = 0;
                while (position < eventsToSave.Count)
                {
                    var pageEvents = eventsToSave.Skip(position).Take(WritePageSize).ToList();
                    await transaction.WriteAsync(pageEvents);
                    _log.Debug("Saved {0} events to stream {1}.", pageEvents.Count, streamName);
                    position += WritePageSize;
                }

                await transaction.CommitAsync();
            }

            foreach (var manageChanges in _manageChanges)
                await manageChanges.ChangesSaved(events, commitHeaders);
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

        protected void OnAggregateLoaded(IAggregate aggregate)
        {
            aggregate.AggregateAttached += OnAggregateLoaded;

            _loadedAggregates[aggregate.Id] = new LoadedAggregate(aggregate);

            var handler = AggregateLoaded;
            if (handler != null) handler(aggregate);
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
            public LoadedAggregate(IAggregate aggregate)
            {
                Aggregate = aggregate;
            }

            public IAggregate Aggregate { get; private set; }
        }
    }
}