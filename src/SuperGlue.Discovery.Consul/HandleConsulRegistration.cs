using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Consul;
using SuperGlue.Configuration;
using SuperGlue.UnitOfWork;

namespace SuperGlue.Discovery.Consul
{
    public class HandleConsulRegistration : IApplicationTask
    {
        public async Task Start(IDictionary<string, object> environment)
        {
            var settings = environment.GetSettings<ConsulServiceSettings>();

            var client = settings.CreateClient();

            await client.Agent.ServiceRegister(new AgentServiceRegistration
            {
                Name = settings.Name,
                ID = settings.Id,
                Address = settings.Address,
                Port = settings.Port,
                Tags = settings.GetTags(),
                Checks = settings.GetChecks()
            }).ConfigureAwait(false);
        }

        public async Task ShutDown(IDictionary<string, object> environment)
        {
            var settings = environment.GetSettings<ConsulServiceSettings>();

            var client = settings.CreateClient();

            await client.Agent.ServiceDeregister(settings.Id).ConfigureAwait(false);
        }

        public Task Exception(IDictionary<string, object> environment, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}