using System;
using System.Text;
using Newtonsoft.Json;

namespace SuperGlue.Caching.Redis
{
    public class DefaultRedisDataSerializer : IRedisDataSerializer
    {
        private static readonly JsonSerializerSettings SerializerSettings;

        static DefaultRedisDataSerializer()
        {
            SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        }

        public virtual byte[] Serialize(object value)
        {
            var data = JsonConvert.SerializeObject(value, SerializerSettings);

            var valueToSerialize = new SerializedObject(data, value.GetType().AssemblyQualifiedName);

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(valueToSerialize, SerializerSettings));
        }

        public virtual object Deserialize(byte[] data)
        {
            var value = JsonConvert.DeserializeObject<SerializedObject>(Encoding.UTF8.GetString(data));

            return JsonConvert.DeserializeObject(value.Data, Type.GetType(value.Type));
        }

        public class SerializedObject
        {
            public SerializedObject(string data, string type)
            {
                Type = type;
                Data = data;
            }

            public string Data { get; private set; }
            public string Type { get; private set; }
        }
    }
}