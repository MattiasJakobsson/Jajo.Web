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
                Tags = settings.GetTags()
            }).ConfigureAwait(false);

	        foreach (var check in settings.GetChecks())
	        {
		        await client.Agent.CheckRegister(new AgentCheckRegistration
		        {
					HTTP = check.HTTP,
			        DeregisterCriticalServiceAfter = check.DeregisterCriticalServiceAfter,
			        DockerContainerID = check.DockerContainerID,
			        Interval = check.Interval,
			        Script = check.Script,
			        ServiceID = settings.Id,
			        Name = $"Service '{settings.Name}' check",
			        ID = $"service:{settings.Id}",
			        Shell = check.Shell,
			        Status = check.Status,
			        TCP = check.TCP,
			        Timeout = check.Timeout,
			        TTL = check.TTL
		        });
	        }
        }

        public Task ShutDown(IDictionary<string, object> environment)
        {
            return Task.CompletedTask;
        }

        public Task Exception(IDictionary<string, object> environment, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}