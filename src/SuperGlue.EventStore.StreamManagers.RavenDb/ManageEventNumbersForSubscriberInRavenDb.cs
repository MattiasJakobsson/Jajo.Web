using System.Threading.Tasks;
using Raven.Client;
using SuperGlue.EventStore.Subscribers;

namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class ManageEventNumbersForSubscriberInRavenDb : IManageEventNumbersForSubscriber
    {
        private readonly IAsyncDocumentSession _session;

        public ManageEventNumbersForSubscriberInRavenDb(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<int?> GetLastEvent(string service, string stream)
        {
            var serviceEventNumber = await GetServiceEventNumber(service, stream);

            return serviceEventNumber.LastEvent;
        }

        public async Task UpdateLastEvent(string service, string stream, int lastEvent)
        {
            var serviceEventNumber = await GetServiceEventNumber(service, stream);

            serviceEventNumber.LastEvent = lastEvent;
        }

        private async Task<ServiceEventNumber> GetServiceEventNumber(string service, string stream)
        {
            var projectionEventNumber = await _session.LoadAsync<ServiceEventNumber>(ServiceEventNumber.GetId(service, stream));

            if (projectionEventNumber == null)
            {
                projectionEventNumber = new ServiceEventNumber
                {
                    Id = ServiceEventNumber.GetId(service, stream)
                };

                await _session.StoreAsync(projectionEventNumber);
            }

            return projectionEventNumber;
        }
    }
}