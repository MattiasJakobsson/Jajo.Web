using System;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore
{
    public class WriteToErrorStream : IWriteToErrorStream
    {
        private readonly IHandleEventSerialization _eventSerialization;

        public WriteToErrorStream(IHandleEventSerialization eventSerialization)
        {
            _eventSerialization = eventSerialization;
        }

        public void Write(object evnt, IEventStoreConnection eventStoreConnection, string errorStreamName)
        {
            if (string.IsNullOrEmpty(errorStreamName))
                return;

            var commitHeaders = new Dictionary<string, object>
            {
                {"CommitId", Guid.NewGuid()}
            };

            var newEvents = new List<object>
            {
                evnt
            };

            var eventsToSave = newEvents.Select(e => ToEventData(Guid.NewGuid(), e, commitHeaders)).ToList();

            eventStoreConnection.AppendToStreamAsync(errorStreamName, ExpectedVersion.Any, eventsToSave);
        }

        private EventData ToEventData(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var serializedEvent = _eventSerialization.Serialize(eventId, evnt, headers);

            return new EventData(serializedEvent.EventId, serializedEvent.Type, serializedEvent.IsJson, serializedEvent.Data, serializedEvent.Metadata);
        }
    }
}