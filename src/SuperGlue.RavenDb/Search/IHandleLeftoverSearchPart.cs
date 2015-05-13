using Raven.Client.Linq;

namespace SuperGlue.RavenDb.Search
{
    public interface IHandleLeftoverSearchPart<TEntity>
    {
        IRavenQueryable<TEntity> ApplyTo(IRavenQueryable<TEntity> query, string key, string value);
    }
}