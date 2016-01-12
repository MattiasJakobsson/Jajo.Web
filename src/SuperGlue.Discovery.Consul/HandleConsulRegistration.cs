using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Consul;
using SuperGlue.UnitOfWork;

namespace SuperGlue.Discovery.Consul
{
    public class HandleConsulRegistration : IApplicationTask
    {
        private readonly ConsulServiceSettings _settings;

        public HandleConsulRegistration(ConsulServiceSettings settings)
        {
            _settings = settings;
        }

        public Task Start(IDictionary<string, object> environment)
        {
            var client = _settings.CreateClient();

            client.Agent.ServiceRegister(new AgentServiceRegistration
            {
                Name = _settings.Name,
                ID = _settings.Id,
                Address = _settings.Address,
                Port = _settings.Port,
                Tags = _settings.GetTags(),
                Checks = _settings.GetChecks(),
            });

            return Task.CompletedTask;
        }

        public Task ShutDown(IDictionary<string, object> environment)
        {
            var client = _settings.CreateClient();

            client.Agent.ServiceDeregister(_settings.Id);

            return Task.CompletedTask;
        }

        public Task Exception(IDictionary<string, object> environment, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}