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

        public async Task<T> Get<T>(string key) where T : class
        {
            return (await Get(key).ConfigureAwait(false)) as T;
        }

        public async Task<object> Get(string key)
        {
            Task<RedisValue> result = null;

            Transactionally(x => result = x.StringGetAsync(key));

            byte[] data = null;

            if (result != null)
                data = await result.ConfigureAwait(false);

            return _redisDataSerializer.Deserialize(data);
        }

        public async Task<IEnumerable<object>> GetList(string key, int? numberOfItems = null)
        {
            Task<RedisValue[]> resultTask = null;

            Transactionally(x => resultTask = x.ListRangeAsync(key, stop: numberOfItems - 1 ?? -1));

            var result = await resultTask.ConfigureAwait(false);

            return result?.Select(x => _redisDataSerializer.Deserialize(x)).ToList() ?? new List<object>();
        }

        public Task<bool> Set(string key, object value, TimeSpan? expires = null)
        {
            var data = _redisDataSerializer.Serialize(value);

            Task<bool> result = null;

            Transactionally(x => result = x.StringSetAsync(key, data, expires));

            return result;
        }

        public Task AddToList(string key, object value, int? maxListLength = null, TimeSpan? expires = null)
        {
            var data = _redisDataSerializer.Serialize(value);

            Task result = null;

            Transactionally(x =>
            {
                result = x
                    .ListLeftPushAsync(key, data)
                    .ContinueWith(y =>
                    {
                        if (maxListLength.HasValue)
                            x.ListTrimAsync(key, 0, maxListLength.Value);
                    })
                    .ContinueWith(y =>
                    {
                        if (expires.HasValue)
                            x.KeyExpireAsync(key, expires.Value);
                    });
            });

            return result;
        }

        public Task AddToList(string key, IEnumerable<object> values, int? maxListLength = null, TimeSpan? expires = null)
        {
            var data = values.Select(x => (RedisValue)_redisDataSerializer.Serialize(x)).ToArray();

            Task result = null;

            Transactionally(x =>
            {
                result = x
                    .ListLeftPushAsync(key, data)
                    .ContinueWith(y =>
                    {
                        if (maxListLength.HasValue)
                            x.ListTrimAsync(key, 0, maxListLength.Value);
                    })
                    .ContinueWith(y =>
                    {
                        if (expires.HasValue)
                            x.KeyExpireAsync(key, expires.Value);
                    });
            });

            return result;
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