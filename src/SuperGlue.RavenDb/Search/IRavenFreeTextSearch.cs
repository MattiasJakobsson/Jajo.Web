using System.Collections.Generic;
using Raven.Client.Linq;

namespace SuperGlue.RavenDb.Search
{
    public interface IRavenFreeTextSearch<TEntity>
    {
        IRavenQueryable<TEntity> ApplyTo(IRavenQueryable<TEntity> query, IEnumerable<string> searches);
    }
}