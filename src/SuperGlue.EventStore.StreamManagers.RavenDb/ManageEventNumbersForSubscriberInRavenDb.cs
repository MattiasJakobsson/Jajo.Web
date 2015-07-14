using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;
using SuperGlue.Configuration;
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

        public async Task<int?> GetLastEvent(string stream, IDictionary<string, object> environment)
        {
            var service = environment.GetApplicationName();

            var serviceEventNumber = await GetServiceEventNumber(service, stream);

            return serviceEventNumber.LastEvent;
        }

        public async Task UpdateLastEvent(string stream, int lastEvent, IDictionary<string, object> environment)
        {
            var service = environment.GetApplicationName();

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