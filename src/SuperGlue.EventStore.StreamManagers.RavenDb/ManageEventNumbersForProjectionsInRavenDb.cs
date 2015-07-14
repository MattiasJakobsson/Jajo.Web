using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;
using SuperGlue.Configuration;
using SuperGlue.EventStore.Projections;

namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class ManageEventNumbersForProjectionsInRavenDb : IManageEventNumbersForProjections
    {
        private readonly IAsyncDocumentSession _session;

        public ManageEventNumbersForProjectionsInRavenDb(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<int?> GetLastEvent(string projection, IDictionary<string, object> environment)
        {
            var projectionEventNumber = await GetProjectionEventNumber(projection, environment);

            return projectionEventNumber.LastEvent;
        }

        public async Task UpdateLastEvent(string projection, int lastEvent, IDictionary<string, object> environment)
        {
            var projectionEventNumber = await GetProjectionEventNumber(projection, environment);

            projectionEventNumber.LastEvent = lastEvent;
        }

        private async Task<ProjectionEventNumber> GetProjectionEventNumber(string projection, IDictionary<string, object> environment)
        {
            var service = environment.GetApplicationName();

            var projectionEventNumber = await _session.LoadAsync<ProjectionEventNumber>(ProjectionEventNumber.GetId(service, projection));

            if (projectionEventNumber == null)
            {
                projectionEventNumber = new ProjectionEventNumber
                {
                    Id = ProjectionEventNumber.GetId(service, projection)
                };

                await _session.StoreAsync(projectionEventNumber);
            }

            return projectionEventNumber;
        }
    }
}