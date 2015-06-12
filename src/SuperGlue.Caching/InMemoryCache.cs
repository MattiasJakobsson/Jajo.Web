using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace SuperGlue.Caching
{
    public class InMemoryCache : ICache
    {
        public T Get<T>(string key) where T : class
        {
            return Get(key) as T;
        }

        public object Get(string key)
        {
            return MemoryCache.Default.Get(key);
        }

        public IEnumerable<object> GetList(string key, int? numberOfItems = null)
        {
            var list = GetListFor(key);

            if (list == null)
                return new List<object>();

            if (numberOfItems == null || !(list.Count > numberOfItems)) 
                return list;

            return list.Take(numberOfItems.Value).ToList();
        }

        public void Set(string key, object value, TimeSpan? expires = null)
        {
            MemoryCache.Default.Set(key, value, new CacheItemPolicy
            {
                AbsoluteExpiration = expires.HasValue ? new DateTimeOffset(DateTime.UtcNow + expires.Value) : ObjectCache.InfiniteAbsoluteExpiration
            });
        }

        public void AddToList(string key, object value, int? maxListLength = null, TimeSpan? expires = null)
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
        }

        public void AddToList(string key, IEnumerable<object> values, int? maxListLength = null, TimeSpan? expires = null)
        {
            foreach (var value in values)
                AddToList(key, value, maxListLength, expires);
        }

        private static ICollection<object> GetListFor(string key)
        {
            return MemoryCache.Default.Get(key) as ICollection<object>;
        }
    }
}