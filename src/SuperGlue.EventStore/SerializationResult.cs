using System;

namespace SuperGlue.EventStore
{
    public class SerializationResult
    {
        public SerializationResult(Guid eventId, string type, bool isJson, byte[] data, byte[] metadata)
        {
            Metadata = metadata;
            Data = data;
            IsJson = isJson;
            Type = type;
            EventId = eventId;
        }

        public Guid EventId { get; private set; }
        public string Type { get; private set; }
        public bool IsJson { get; private set; }
        public byte[] Data { get; private set; }
        public byte[] Metadata { get; private set; }
    }
}