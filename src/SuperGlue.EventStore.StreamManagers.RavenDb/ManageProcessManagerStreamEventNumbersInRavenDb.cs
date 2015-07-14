using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;
using SuperGlue.Configuration;
using SuperGlue.EventStore.ProcessManagers;

namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class ManageProcessManagerStreamEventNumbersInRavenDb : IManageProcessManagerStreamEventNumbers
    {
        private readonly IAsyncDocumentSession _session;

        public ManageProcessManagerStreamEventNumbersInRavenDb(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<int?> GetLastEvent(string processManager, IDictionary<string, object> environment)
        {
            var serviceEventNumber = await GetProcessManagerEventNumber(processManager, environment);

            return serviceEventNumber.LastEvent;
        }

        public async Task UpdateLastEvent(string processManager, int lastEvent, IDictionary<string, object> environment)
        {
            var serviceEventNumber = await GetProcessManagerEventNumber(processManager, environment);

            serviceEventNumber.LastEvent = lastEvent;
        }

        private async Task<ProcessManagerEventNumber> GetProcessManagerEventNumber(string processManager, IDictionary<string, object> environment)
        {
            var service = environment.GetApplicationName();

            var serviceEventNumber = await _session.LoadAsync<ProcessManagerEventNumber>(ProcessManagerEventNumber.GetId(service, processManager));

            if (serviceEventNumber == null)
            {
                serviceEventNumber = new ProcessManagerEventNumber
                {
                    Id = ProcessManagerEventNumber.GetId(service, processManager)
                };

                await _session.StoreAsync(serviceEventNumber);
            }

            return serviceEventNumber;
        }
    }
}