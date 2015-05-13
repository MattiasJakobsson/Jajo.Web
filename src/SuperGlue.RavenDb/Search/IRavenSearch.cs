using Raven.Client.Linq;

namespace SuperGlue.RavenDb.Search
{
    public interface IRavenSearch
    {
        IRavenQueryable<TEntity> Search<TEntity>(IRavenQueryable<TEntity> query, string search, params string[] specialCommands);
    }
}