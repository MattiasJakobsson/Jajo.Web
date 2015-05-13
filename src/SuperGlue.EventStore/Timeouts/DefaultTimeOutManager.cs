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

        public void RequestTimeOut(string writeTo, Guid id, object message, DateTime time, IDictionary<string, object> metaData)
        {
            var timeOutManager = TimeOutManager.GetCurrent();

            if (timeOutManager == null)
                throw new InvalidOperationException("There is no timeout manager");

            timeOutManager.Add(new TimeoutData(writeTo, id, message, time, metaData));
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();

            StartPoller();
        }

        public void Stop()
        {
            if (_tokenSource != null)
                _tokenSource.Cancel();
        }

        private void StartPoller()
        {
            var token = _tokenSource.Token;

            Task.Factory
                .StartNew(Poll, token, TaskCreationOptions.LongRunning)
                .ContinueWith(t =>
                {
                    t.Exception.Handle(ex => true);

                    StartPoller();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void Poll(object obj)
        {
            var cancellationToken = (CancellationToken)obj;

            var startSlice = DateTime.UtcNow.AddYears(-10);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_nextRetrieval > DateTime.UtcNow)
                {
                    Thread.Sleep(_secondsToSleepBetweenPolls * 1000);
                    continue;
                }

                var timeOutManager = TimeOutManager.GetCurrent();

                if (timeOutManager == null)
                    return;

                DateTime nextExpiredTimeout;
                timeOutManager.GetNextChunk(startSlice, x =>
                {
                    if (startSlice < x.Item2)
                        startSlice = x.Item2;

                    _eventStoreConnection.AppendToStreamAsync(x.Item1.WriteTo, ExpectedVersion.Any, ToEventData(x.Item1.Id, x.Item1.Message, x.Item1.MetaData));
                }, out nextExpiredTimeout);

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