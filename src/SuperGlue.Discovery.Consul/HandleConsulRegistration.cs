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
        public Task Start(IDictionary<string, object> environment)
        {
            var settings = environment.GetSettings<ConsulServiceSettings>();

            var client = settings.CreateClient();

            client.Agent.ServiceRegister(new AgentServiceRegistration
            {
                Name = settings.Name,
                ID = settings.Id,
                Address = settings.Address,
                Port = settings.Port,
                Tags = settings.GetTags(),
                Checks = settings.GetChecks(),
            });

            return Task.CompletedTask;
        }

        public Task ShutDown(IDictionary<string, object> environment)
        {
            var settings = environment.GetSettings<ConsulServiceSettings>();

            var client = settings.CreateClient();

            client.Agent.ServiceDeregister(settings.Id);

            return Task.CompletedTask;
        }

        public Task Exception(IDictionary<string, object> environment, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}