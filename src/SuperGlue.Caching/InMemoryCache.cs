using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SuperGlue.Caching
{
    public class InMemoryCache : ICache
    {
        public async Task<T> Get<T>(string key) where T : class
        {
            return (await Get(key)) as T;
        }

        public Task<object> Get(string key)
        {
            return Task.FromResult(MemoryCache.Default.Get(key));
        }

        public Task<IEnumerable<object>> GetList(string key, int? numberOfItems = null)
        {
            var list = GetListFor(key);

            if (list == null)
                return Task.FromResult(Enumerable.Empty<object>());

            if (numberOfItems == null || !(list.Count > numberOfItems)) 
                return Task.FromResult(list.AsEnumerable());

            return Task.FromResult(list.Take(numberOfItems.Value));
        }

        public Task<bool> Set(string key, object value, TimeSpan? expires = null)
        {
            MemoryCache.Default.Set(key, value, new CacheItemPolicy
            {
                AbsoluteExpiration = expires.HasValue ? new DateTimeOffset(DateTime.UtcNow + expires.Value) : ObjectCache.InfiniteAbsoluteExpiration
            });

            return Task.FromResult(true);
        }

        public Task AddToList(string key, object value, int? maxListLength = null, TimeSpan? expires = null)
        {
            var list = GetListFor(key);

            if (list == null)
            {
                list = new List<object>();
                Set(key, list, expires);
            }

            list.Add(value);

            if (maxListLength != null && list.Count > maxListLength)
                list = list.Take(maxListLength.Value).ToList();

            Set(key, list, expires);

            return Task.CompletedTask;
        }

        public async Task AddToList(string key, IEnumerable<object> values, int? maxListLength = null, TimeSpan? expires = null)
        {
            foreach (var value in values)
                await AddToList(key, value, maxListLength, expires);
        }

        private static ICollection<object> GetListFor(string key)
        {
            return MemoryCache.Default.Get(key) as ICollection<object>;
        }
    }
}