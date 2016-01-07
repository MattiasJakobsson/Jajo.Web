namespace SuperGlue.Caching.Redis
{
    public class RedisCacheSettings
    {
        public string ConnectionString { get; private set; }

        public RedisCacheSettings UseConnectionString(string connectionString)
        {
            ConnectionString = connectionString;

            return this;
        }
    }
}