using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public class DeSerializationResult
    {
        public DeSerializationResult(Guid eventId, int eventNumber, int originalEventNumber, object data, Dictionary<string, object> metadata, Exception error = null)
        {
            OriginalEventNumber = originalEventNumber;
            EventNumber = eventNumber;
            Error = error;
            Metadata = metadata;
            Data = data;
            EventId = eventId;
        }

        public Guid EventId { get; private set; }
        public int EventNumber { get; private set; }
        public int OriginalEventNumber { get; private set; }
        public object Data { get; private set; }
        public Dictionary<string, object> Metadata { get; private set; }
        public Exception Error { get; private set; }
        public bool Successful { get { return Error == null; } }
    }
}