using System;
using System.Threading.Tasks;
using Raven.Client;
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

        public async Task<int?> GetLastEvent(string processManager)
        {
            var serviceEventNumber = await GetProcessManagerEventNumber(processManager);

            return serviceEventNumber.LastEvent;
        }

        public async Task UpdateLastEvent(string processManager, int lastEvent)
        {
            var serviceEventNumber = await GetProcessManagerEventNumber(processManager);

            serviceEventNumber.LastEvent = lastEvent;
        }

        private async Task<ProcessManagerEventNumber> GetProcessManagerEventNumber(string processManager)
        {
            var serviceEventNumber = await _session.LoadAsync<ProcessManagerEventNumber>(ProcessManagerEventNumber.GetId(processManager));

            if (serviceEventNumber == null)
            {
                serviceEventNumber = new ProcessManagerEventNumber
                {
                    Id = ProcessManagerEventNumber.GetId(processManager)
                };

                await _session.StoreAsync(serviceEventNumber);
            }

            return serviceEventNumber;
        }
    }
}