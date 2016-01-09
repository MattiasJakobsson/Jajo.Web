using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    public interface IProjectionsInstaller
    {
        Task InstallProjectionFor<TProjection>() where TProjection : IEventStoreProjection;
    }
}