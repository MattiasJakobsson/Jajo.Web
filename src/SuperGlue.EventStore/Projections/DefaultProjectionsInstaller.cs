using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.EventStore.Data;

namespace SuperGlue.EventStore.Projections
{
    public class DefaultProjectionsInstaller : IProjectionsInstaller
    {
        private readonly IEnumerable<IEventStoreProjection> _projections;
        private readonly EventStoreConnectionString _eventStoreConnectionString;

        public DefaultProjectionsInstaller(IEnumerable<IEventStoreProjection> projections, EventStoreConnectionString eventStoreConnectionString)
        {
            _projections = projections;
            _eventStoreConnectionString = eventStoreConnectionString;
        }

        public async Task InstallProjectionFor<TProjection>() where TProjection : IEventStoreProjection
        {
            var matchingProjections = _projections.OfType<TProjection>().ToList();

            var projectionBuilder = new ProjectionBuilder();
            var projectionManager = _eventStoreConnectionString.CreateProjectionsManager();
            var credentials = _eventStoreConnectionString.GetUserCredentials();

            foreach (var projection in matchingProjections)
            {
                var name = $"project-to-{projection.ProjectionName}";
                var query = projectionBuilder.BuildStreamProjection(projection.GetInterestingStreams(), projection.ProjectionName);

                await projectionManager.CreateOrUpdateContinuousQueryAsync(name, query, credentials).ConfigureAwait(false);
            }
        }
    }
}