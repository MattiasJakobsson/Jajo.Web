using Raven.Client.Linq;

namespace SuperGlue.RavenDb.Search
{
    public interface IRavenSpecialCommandSearch<TEntity>
    {
        string Command { get; }

        IRavenQueryable<TEntity> ApplyTo(IRavenQueryable<TEntity> query);
    }
}