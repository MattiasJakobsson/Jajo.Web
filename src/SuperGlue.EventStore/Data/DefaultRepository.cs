using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using SuperGlue.Configuration;
using SuperGlue.EventStore.ConflictManagement;
using SuperGlue.EventStore.ProcessManagers;
using SuperGlue.EventStore.Timeouts;
using SuperGlue.EventTracking;
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
        private readonly IDictionary<string, object> _environment;
        private readonly ConcurrentDictionary<string, LoadedAggregate> _loadedAggregates = new ConcurrentDictionary<string, LoadedAggregate>();
        private readonly ConcurrentStack<LoadedEventAwareItem> _loadedEventAwareItems = new ConcurrentStack<LoadedEventAwareItem>();
        private readonly ConcurrentDictionary<string, LoadedProcessState> _loadedProcessStates = new ConcurrentDictionary<string, LoadedProcessState>();
        private readonly ConcurrentStack<AttachedCommand> _attachedCommands = new ConcurrentStack<AttachedCommand>();

        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;

        public const string CorrelationIdKey = "CorrelationId";
        public const string CausationIdKey = "CausationId";
        public const string CommitIdHeader = "CommitId";
        public const string AggregateClrTypeHeader = "AggregateClrTypeName";
        public const string AggregateIdHeader = "AggregateId";
        public const string EntityClrTypeHeader = "EntityClrTypeName";
        public const string EntityIdHeader = "EntityId";

        public DefaultRepository(IEventStoreConnection eventStoreConnection, IHandleEventSerialization eventSerialization, ICheckConflicts checkConflicts,
            IEnumerable<IManageChanges> manageChanges, IManageTimeOuts timeoutManager, IDictionary<string, object> environment)
        {
            _eventStoreConnection = eventStoreConnection;
            _eventSerialization = eventSerialization;
            _checkConflicts = checkConflicts;
            _manageChanges = manageChanges;
            _timeoutManager = timeoutManager;
            _environment = environment;
        }

        public async Task<T> Load<T>(string id) where T : IAggregate, new()
        {
            LoadedAggregate loadedAggregate;

            if (_loadedAggregates.TryGetValue(id, out loadedAggregate))
                return (T)loadedAggregate.Aggregate;

            var aggregate = new T
            {
                Id = id
            };

            var streamName = aggregate.GetStreamName(_environment);

            var events = await LoadEventsFromStream(streamName, 0, int.MaxValue).ConfigureAwait(false);

            aggregate.BuildFromHistory(new EventStream(events.Select(x => new Event(x.Event.EventId, DeserializeEvent(x)))));

            OnAggregateLoaded(aggregate);

            return aggregate;
        }

        public async Task<T> Load<T>(string streamName, string id) where T : IProcessManagerState, new()
        {
            LoadedProcessState loadedState;
            if (_loadedProcessStates.TryGetValue(streamName, out loadedState))
                return (T)loadedState.State;

            var state = new T
            {
                Id = id
            };

            var events = await LoadEventsFromStream(streamName, 0, int.MaxValue).ConfigureAwait(false);

            state.BuildFromHistory(new EventStream(events.Select(x => new Event(x.Event.EventId, DeserializeEvent(x)))));

            OnProcessStateLoaded(state, streamName);

            return state;
        }

        public async Task<IEnumerable<object>> LoadStream(string stream)
        {
            return (await LoadEventsFromStream(stream, 0, int.MaxValue).ConfigureAwait(false)).Select(DeserializeEvent);
        }

        public async Task RequestTimeOut(string stream, object evnt, DateTime at)
        {
            var commitHeaders = new Dictionary<string, object>();

            var metaData = _environment.GetMetaData().MetaData;

            foreach (var item in metaData)
                commitHeaders[item.Key] = item.Value;

            var correlationId = _environment.GetCorrelationId();
            var causationId = _environment.GetCausationId();

            if (!string.IsNullOrEmpty(correlationId))
                commitHeaders[CorrelationIdKey] = correlationId;

            if (!string.IsNullOrEmpty(causationId))
                commitHeaders[CausationIdKey] = causationId;

            await _timeoutManager.RequestTimeOut(stream, Guid.NewGuid(), evnt, at, commitHeaders).ConfigureAwait(false);
        }

        public Task RequestTimeOut(string stream, object evnt, TimeSpan @in)
        {
            return RequestTimeOut(stream, evnt, DateTime.UtcNow + @in);
        }

        public async Task SaveChanges()
        {
            foreach (var loadedProcessState in _loadedProcessStates)
                await Save(loadedProcessState.Value.State, loadedProcessState.Key, loadedProcessState.Value.CorrelationId, loadedProcessState.Value.CausationId).ConfigureAwait(false);

            foreach (var aggregate in _loadedAggregates)
                await Save(aggregate.Value.Aggregate, aggregate.Value.CorrelationId, aggregate.Value.CausationId).ConfigureAwait(false);

            LoadedEventAwareItem item;
            while (_loadedEventAwareItems.TryPop(out item))
                await Save(item.CanApplyEvents, item.CorrelationId, item.CausationId).ConfigureAwait(false);

            AttachedCommand command;
            while (_attachedCommands.TryPop(out command))
                await Save(command.Command, command.Id, command.CorrelationId, command.CausationId).ConfigureAwait(false);
        }

        public void ThrowAwayChanges()
        {
            _loadedAggregates.Clear();
        }

        private async Task Save(IAggregate aggregate, string correlationId, string causationId)
        {
            var commitHeaders = new Dictionary<string, object>();

            var metaData = _environment.GetMetaData().MetaData;

            foreach (var item in metaData)
                commitHeaders[item.Key] = item.Value;

            if (!string.IsNullOrEmpty(correlationId))
                commitHeaders[CorrelationIdKey] = correlationId;

            if (!string.IsNullOrEmpty(causationId))
                commitHeaders[CausationIdKey] = causationId;

            commitHeaders[AggregateClrTypeHeader] = aggregate.GetType().AssemblyQualifiedName;
            commitHeaders[AggregateIdHeader] = aggregate.Id;

            _environment.GetSettings<EventStoreSettings>().ModifyHeaders(commitHeaders);

            var streamName = aggregate.GetStreamName(_environment);
            var eventStream = aggregate.GetUncommittedChanges();
            var newEvents = eventStream.Events.ToList();
            var originalVersion = aggregate.Version - newEvents.Count;

            var versionToExpect = originalVersion == 0 ? ExpectedVersion.Any : originalVersion - 1;

            while (true)
            {
                try
                {
                    await SaveEventsToStream(streamName, versionToExpect, newEvents, commitHeaders).ConfigureAwait(false);
                    break;
                }
                catch (WrongExpectedVersionException ex)
                {
                    _environment.Log(ex, "Events where added to aggregate with id: {0} since last load. Checking for conflicts and trying again...", LogLevel.Warn, aggregate.Id);
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is WrongExpectedVersionException))
                        throw;

                    _environment.Log(ae.InnerException, "Events where added to aggregate with id: {0} since last load. Checking for conflicts and trying again...", LogLevel.Warn, aggregate.Id);
                }

                var storedEvents = (await LoadEventsFromStream(streamName, versionToExpect < 0 ? 0 : versionToExpect, int.MaxValue).ConfigureAwait(false)).ToList();

                var currentVersion = storedEvents.Select(x => x.OriginalEventNumber).OrderByDescending(x => x).FirstOrDefault();

                if (_checkConflicts.HasConflicts(newEvents, storedEvents, _environment))
                    throw new ConflictingEventException(streamName, versionToExpect, currentVersion);

                versionToExpect = currentVersion;
            }

            aggregate.ClearUncommittedChanges();
        }

        private async Task Save(ICanApplyEvents canApplyEvents, string correlationId, string causationId)
        {
            var commitHeaders = new Dictionary<string, object>();

            var metaData = _environment.GetMetaData().MetaData;

            foreach (var item in metaData)
                commitHeaders[item.Key] = item.Value;

            if (!string.IsNullOrEmpty(correlationId))
                commitHeaders[CorrelationIdKey] = correlationId;

            if (!string.IsNullOrEmpty(causationId))
                commitHeaders[CausationIdKey] = causationId;

            commitHeaders[EntityClrTypeHeader] = canApplyEvents.GetType().AssemblyQualifiedName;
            commitHeaders[EntityIdHeader] = canApplyEvents.Id;

            _environment.GetSettings<EventStoreSettings>().ModifyHeaders(commitHeaders);

            var streamName = canApplyEvents.GetStreamName(_environment);
            var events = canApplyEvents.GetAppliedEvents().ToList();

            await SaveEventsToStream(streamName, ExpectedVersion.Any, events.Select(x => new Event(x.Id, x.Instance)).ToList(), commitHeaders).ConfigureAwait(false);

            canApplyEvents.ClearAppliedEvents();
        }

        private async Task Save(IProcessManagerState state, string streamName, string correlationId, string causationId)
        {
            var commitHeaders = new Dictionary<string, object>();

            var metaData = _environment.GetMetaData().MetaData;

            foreach (var item in metaData)
                commitHeaders[item.Key] = item.Value;

            if (!string.IsNullOrEmpty(correlationId))
                commitHeaders[CorrelationIdKey] = correlationId;

            if (!string.IsNullOrEmpty(causationId))
                commitHeaders[CausationIdKey] = causationId;

            commitHeaders[AggregateClrTypeHeader] = state.GetType().AssemblyQualifiedName;
            commitHeaders[AggregateIdHeader] = state.Id;

            _environment.GetSettings<EventStoreSettings>().ModifyHeaders(commitHeaders);

            var eventStream = state.GetUncommittedChanges();

            var newEvents = eventStream.Events.ToList();
            var originalVersion = state.Version - newEvents.Count;

            var versionToExpect = originalVersion == 0 ? ExpectedVersion.Any : originalVersion - 1;

            await SaveEventsToStream(streamName, versionToExpect, newEvents, commitHeaders).ConfigureAwait(false);

            state.ClearUncommittedChanges();
        }

        private async Task Save(object command, Guid id, string correlationId, string causationId)
        {
            var settings = _environment.GetSettings<EventStoreSettings>();

            var streamName = (settings.FindCommandStreamFor ?? ((x, y, z, a) => null))(_environment, command, id, causationId);

            if (string.IsNullOrWhiteSpace(streamName))
                return;

            var commitHeaders = new Dictionary<string, object>();

            var metaData = _environment.GetMetaData().MetaData;

            foreach (var item in metaData)
                commitHeaders[item.Key] = item.Value;

            if (!string.IsNullOrEmpty(correlationId))
                commitHeaders[CorrelationIdKey] = correlationId;

            if (!string.IsNullOrEmpty(causationId))
                commitHeaders[CausationIdKey] = causationId;

            await SaveEventsToStream(streamName, ExpectedVersion.Any, new List<Event> { new Event(id, command) }, commitHeaders).ConfigureAwait(false);
        }

        public void Attach(IAggregate aggregate)
        {
            OnAggregateLoaded(aggregate);
        }

        public void Attach(ICanApplyEvents canApplyEvents)
        {
            _loadedEventAwareItems.Push(new LoadedEventAwareItem(canApplyEvents, _environment.GetCorrelationId(), _environment.GetCausationId()));
        }

        public void Attach(object command, Guid id, string causedBy)
        {
            _attachedCommands.Push(new AttachedCommand(command, id, _environment.GetCorrelationId(), causedBy));
        }

        protected async Task SaveEventsToStream(string streamName, int expectedVersion, IReadOnlyCollection<Event> events, IDictionary<string, object> commitHeaders)
        {
            commitHeaders[CommitIdHeader] = Guid.NewGuid();

            var eventsToSave = events.Select(e => ToEventData(e.Id, e.Instance, commitHeaders)).ToList();

            if (!eventsToSave.Any())
                return;

            if (eventsToSave.Count < WritePageSize)
            {
                await _eventStoreConnection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave).ConfigureAwait(false);
                _environment.Log("Saved {0} events to stream {1}.", LogLevel.Debug, eventsToSave.Count, streamName);
            }
            else
            {
                var transaction = await _eventStoreConnection.StartTransactionAsync(streamName, expectedVersion).ConfigureAwait(false);

                var position = 0;
                while (position < eventsToSave.Count)
                {
                    var pageEvents = eventsToSave.Skip(position).Take(WritePageSize).ToList();
                    await transaction.WriteAsync(pageEvents).ConfigureAwait(false);
                    _environment.Log("Saved {0} events to stream {1}.", LogLevel.Debug, pageEvents.Count, streamName);
                    position += WritePageSize;
                }

                await transaction.CommitAsync().ConfigureAwait(false);
            }

            foreach (var manageChanges in _manageChanges)
                await manageChanges.ChangesSaved(events, commitHeaders).ConfigureAwait(false);
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

                currentSlice = await _eventStoreConnection.ReadStreamEventsForwardAsync(streamName, sliceStart, sliceCount, false).ConfigureAwait(false);

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

            _loadedAggregates[aggregate.Id] = new LoadedAggregate(aggregate, _environment.GetCorrelationId(), _environment.GetCausationId());
        }

        protected void OnProcessStateLoaded(IProcessManagerState state, string stream)
        {
            _loadedProcessStates[stream] = new LoadedProcessState(state, _environment.GetCorrelationId(), _environment.GetCausationId());
        }

        private EventData ToEventData(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var serializedEvent = _eventSerialization.Serialize(eventId, evnt, headers);

            return new EventData(serializedEvent.EventId, serializedEvent.Type, serializedEvent.IsJson, serializedEvent.Data, serializedEvent.Metadata);
        }

        private object DeserializeEvent(ResolvedEvent evnt)
        {
            return _eventSerialization.DeSerialize(evnt).Data;
        }

        private class LoadedAggregate
        {
            public LoadedAggregate(IAggregate aggregate, string correlationId, string causationId)
            {
                Aggregate = aggregate;
                CorrelationId = correlationId;
                CausationId = causationId;
            }

            public IAggregate Aggregate { get; }
            public string CorrelationId { get; }
            public string CausationId { get; }
        }

        private class LoadedEventAwareItem
        {
            public LoadedEventAwareItem(ICanApplyEvents canApplyEvents, string correlationId, string causationId)
            {
                CanApplyEvents = canApplyEvents;
                CorrelationId = correlationId;
                CausationId = causationId;
            }

            public ICanApplyEvents CanApplyEvents { get; }
            public string CorrelationId { get; }
            public string CausationId { get; }
        }

        private class LoadedProcessState
        {
            public LoadedProcessState(IProcessManagerState state, string correlationId, string causationId)
            {
                State = state;
                CorrelationId = correlationId;
                CausationId = causationId;
            }

            public IProcessManagerState State { get; }
            public string CorrelationId { get; }
            public string CausationId { get; }
        }

        private class AttachedCommand
        {
            public AttachedCommand(object command, Guid id, string correlationId, string causationId)
            {
                Command = command;
                Id = id;
                CorrelationId = correlationId;
                CausationId = causationId;
            }

            public object Command { get; }
            public Guid Id { get; }
            public string CorrelationId { get; }
            public string CausationId { get; }
        }
    }
}