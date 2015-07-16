using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SuperGlue.EventStore
{
    public class DefaultEventSerializer : IHandleEventSerialization
    {
        private static readonly JsonSerializerSettings SerializerSettings;
        private const string EventClrTypeHeader = "EventClrTypeName";

        static DefaultEventSerializer()
        {
            SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        }

        public SerializationResult Serialize(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, SerializerSettings));

            var eventHeaders = new Dictionary<string, object>(headers);

            eventHeaders[EventClrTypeHeader] = evnt.GetType().AssemblyQualifiedName;

            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, SerializerSettings));
            var typeName = evnt.GetType().Name;

            return new SerializationResult(eventId, typeName, true, data, metadata);
        }

        public DeSerializationResult DeSerialize(Guid eventId, int eventNumber, int originalEventNumber, byte[] metadata, byte[] data)
        {
            try
            {
                var deSerializedMetaData = (Dictionary<string, object>)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(metadata), typeof(Dictionary<string, object>));

                var deSerializedData = deSerializedMetaData.ContainsKey(EventClrTypeHeader) ? JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)deSerializedMetaData[EventClrTypeHeader])) : new EventStoreEvent();

                return new DeSerializationResult(eventId, eventNumber, originalEventNumber, deSerializedData, deSerializedMetaData);
            }
            catch (Exception exception)
            {
                return new DeSerializationResult(eventId, 0, 0, null, new Dictionary<string, object>(), exception);
            }
        }

        public class EventStoreEvent
        {

        }
    }
}