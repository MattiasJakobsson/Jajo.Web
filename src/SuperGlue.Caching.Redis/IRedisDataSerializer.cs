namespace SuperGlue.Caching.Redis
{
    public interface IRedisDataSerializer
    {
        byte[] Serialize(object value);

        object Deserialize(byte[] data);
    }
}