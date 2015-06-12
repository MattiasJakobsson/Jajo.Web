using System;
using System.Collections.Generic;

namespace SuperGlue.Caching
{
    public interface ICache
    {
        T Get<T>(string key) where T : class;

        object Get(string key);

        IEnumerable<object> GetList(string key, int? numberOfItems = null);

        void Set(string key, object value, TimeSpan? expires = null);

        void AddToList(string key, object value, int? maxListLength = null, TimeSpan? expires = null);

        void AddToList(string key, IEnumerable<object> values, int? maxListLength = null, TimeSpan? expires = null);
    }
}
