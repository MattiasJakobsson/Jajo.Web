using System.Threading.Tasks;
using Raven.Client;
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

        public async Task<int?> GetLastEvent(string projection)
        {
            var projectionEventNumber = await GetProjectionEventNumber(projection);

            return projectionEventNumber.LastEvent;
        }

        public async Task UpdateLastEvent(string projection, int lastEvent)
        {
            var projectionEventNumber = await GetProjectionEventNumber(projection);

            projectionEventNumber.LastEvent = lastEvent;
        }

        private async Task<ProjectionEventNumber> GetProjectionEventNumber(string projection)
        {
            var projectionEventNumber = await _session.LoadAsync<ProjectionEventNumber>(ProjectionEventNumber.GetId(projection));

            if (projectionEventNumber == null)
            {
                projectionEventNumber = new ProjectionEventNumber
                {
                    Id = ProjectionEventNumber.GetId(projection)
                };

                await _session.StoreAsync(projectionEventNumber);
            }

            return projectionEventNumber;
        }
    }
}