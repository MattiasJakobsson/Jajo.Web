using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Caching
{
    public interface ICache
    {
        Task<T> Get<T>(string key) where T : class;

        Task<object> Get(string key);

        Task<IEnumerable<object>> GetList(string key, int? numberOfItems = null);

        Task<bool> Set(string key, object value, TimeSpan? expires = null);

        Task AddToList(string key, object value, int? maxListLength = null, TimeSpan? expires = null);

        Task AddToList(string key, IEnumerable<object> values, int? maxListLength = null, TimeSpan? expires = null);
    }
}
