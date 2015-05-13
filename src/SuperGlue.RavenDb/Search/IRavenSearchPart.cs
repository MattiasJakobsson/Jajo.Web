using Raven.Client.Linq;

namespace SuperGlue.RavenDb.Search
{
    public interface IRavenSearchPart<TEntity>
    {
        string Part { get; }

        IRavenQueryable<TEntity> ApplyTo(IRavenQueryable<TEntity> query, string value);
    }
}