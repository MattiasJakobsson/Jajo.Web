using System;
using System.Collections.Generic;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore
{
    public interface IHandleEventSerialization
    {
        SerializationResult Serialize(Guid eventId, object evnt, IDictionary<string, object> headers);
        DeSerializationResult DeSerialize(ResolvedEvent resolvedEvent);
    }
}