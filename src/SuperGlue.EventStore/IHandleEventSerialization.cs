using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public interface IHandleEventSerialization
    {
        SerializationResult Serialize(Guid eventId, object evnt, IDictionary<string, object> headers);
        DeSerializationResult DeSerialize(Guid eventId, int eventNumber, int originalEventNumber, byte[] metadata, byte[] data);
    }
}