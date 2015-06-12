using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace SuperGlue.Caching.Redis
{
    public class RedisCacheProvider : ICache
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IRedisDataSerializer _redisDataSerializer;

        public RedisCacheProvider(ConnectionMultiplexer connectionMultiplexer, IRedisDataSerializer redisDataSerializer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _redisDataSerializer = redisDataSerializer;
        }

        public T Get<T>(string key) where T : class
        {
            return Get(key) as T;
        }

        public object Get(string key)
        {
            Task<RedisValue> result = null;

            Transactionally(x => result = x.StringGetAsync(key));

            byte[] data = null;

            if (result != null)
                data = result.Result;

            return _redisDataSerializer.Deserialize(data);
        }

        public IEnumerable<object> GetList(string key, int? numberOfItems = null)
        {
            Task<RedisValue[]> result = null;

            Transactionally(x => result = x.ListRangeAsync(key, stop: numberOfItems - 1 ?? -1));

            return result == null ? new List<object>() : result.Result.Select(x => _redisDataSerializer.Deserialize(x)).ToList();
        }

        public void Set(string key, object value, TimeSpan? expires = null)
        {
            var data = _redisDataSerializer.Serialize(value);

            Transactionally(x => x.StringSetAsync(key, data, expires));
        }

        public void AddToList(string key, object value, int? maxListLength = null, TimeSpan? expires = null)
        {
            var data = _redisDataSerializer.Serialize(value);

            Transactionally(x =>
            {
                x.ListLeftPushAsync(key, data);

                if (maxListLength.HasValue)
                    x.ListTrimAsync(key, 0, maxListLength.Value);

                if (expires.HasValue)
                    x.KeyExpireAsync(key, expires.Value);
            });
        }

        public void AddToList(string key, IEnumerable<object> values, int? maxListLength = null, TimeSpan? expires = null)
        {
            var data = values.Select(x => (RedisValue)_redisDataSerializer.Serialize(x)).ToArray();

            Transactionally(x =>
            {
                x.ListLeftPushAsync(key, data);

                if (maxListLength.HasValue)
                    x.ListTrimAsync(key, 0, maxListLength.Value);

                if (expires.HasValue)
                    x.KeyExpireAsync(key, expires.Value);
            });
        }

        private void Transactionally(Action<ITransaction> action)
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();

                var transaction = database.CreateTransaction();

                action(transaction);

                transaction.Execute();
            }
            catch (Exception exception)
            {
                if (exception is RedisConnectionException)
                    return;

                throw;
            }
        }
    }
}