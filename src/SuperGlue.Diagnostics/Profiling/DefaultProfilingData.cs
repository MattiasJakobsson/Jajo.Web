using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Diagnostics.Profiling
{
    public class DefaultProfilingData : IProfilingData
    {
        private readonly ConcurrentLruLSet<KeyValuePair<Guid, IProfilingContext>> _leastRecentlyUsedCache = new ConcurrentLruLSet<KeyValuePair<Guid, IProfilingContext>>(128);

        public void AddContextFor(Guid id, IProfilingContext context)
        {
            _leastRecentlyUsedCache.Push(new KeyValuePair<Guid, IProfilingContext>(id, context));
        }

        public IEnumerable<ProfilingInformation> GetFor(Guid id)
        {
            var contexts = _leastRecentlyUsedCache.Where(x => x.Key == id).Select(x => x.Value).ToList();

            return contexts.Select(context => context.GetProfilingInformation());
        }
    }
}