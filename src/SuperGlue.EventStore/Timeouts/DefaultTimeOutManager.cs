using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.Timeouts
{
    public class DefaultTimeOutManager : IManageTimeOuts
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IEventStoreConnection _eventStoreConnection;
        private CancellationTokenSource _tokenSource;
        private DateTime _nextRetrieval = DateTime.UtcNow;
        private readonly int _secondsToSleepBetweenPolls;
        private readonly object _lockObject = new object();

        public DefaultTimeOutManager(IHandleEventSerialization eventSerialization, IEventStoreConnection eventStoreConnection)
        {
            _eventSerialization = eventSerialization;
            _eventStoreConnection = eventStoreConnection;
            _secondsToSleepBetweenPolls = 1;
        }

        public async Task RequestTimeOut(string writeTo, Guid id, object message, DateTime time, IDictionary<string, object> metaData)
        {
            var timeOutManager = TimeOutManager.GetCurrent();

            if (timeOutManager == null)
                throw new InvalidOperationException("There is no timeout manager");

            await timeOutManager.Add(new TimeoutData(writeTo, id, message, time, metaData)).ConfigureAwait(false);
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();

            StartPoller();
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
        }

        private void StartPoller()
        {
            var token = _tokenSource.Token;

            Task.Factory
                .StartNew(async x => await Poll(x).ConfigureAwait(false), token, TaskCreationOptions.LongRunning)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);

                    StartPoller();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task Poll(object obj)
        {
            var cancellationToken = (CancellationToken)obj;

            var startSlice = DateTime.UtcNow.AddYears(-10);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_nextRetrieval > DateTime.UtcNow)
                {
                    await Task.Delay(_secondsToSleepBetweenPolls * 1000, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                var timeOutManager = TimeOutManager.GetCurrent();

                if (timeOutManager == null)
                    return;

                var nextExpiredTimeout = await timeOutManager.GetNextChunk(startSlice, x =>
                {
                    if (startSlice < x.Item2)
                        startSlice = x.Item2;

                    return _eventStoreConnection.AppendToStreamAsync(x.Item1.WriteTo, ExpectedVersion.Any, ToEventData(x.Item1.Id, x.Item1.Message, x.Item1.MetaData));
                }).ConfigureAwait(false);

                _nextRetrieval = nextExpiredTimeout;

                lock (_lockObject)
                {
                    if (nextExpiredTimeout < _nextRetrieval)
                        _nextRetrieval = nextExpiredTimeout;
                }

                var maxNextRetrieval = DateTime.UtcNow + TimeSpan.FromMinutes(1);

                if (_nextRetrieval > maxNextRetrieval)
                    _nextRetrieval = maxNextRetrieval;
            }
        }

        private EventData ToEventData(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var serializedEvent = _eventSerialization.Serialize(eventId, evnt, headers);

            return new EventData(serializedEvent.EventId, serializedEvent.Type, serializedEvent.IsJson, serializedEvent.Data, serializedEvent.Metadata);
        }
    }
}