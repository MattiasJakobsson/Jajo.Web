using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Monitoring;

namespace SuperGlue.Discovery.Consul.Checks.Ttl
{
    public class HandleConsulTtlCheck : IMonitorHeartBeats
    {
        public Task Beat(IDictionary<string, object> environment)
        {
            var consulServiceSettings = environment.GetSettings<ConsulServiceSettings>();
            var checkSettings = environment.GetSettings<ConsulTtlCheckSettings>();

            var client = consulServiceSettings.CreateClient();

            var response = checkSettings.PerformCheck(environment);

            switch (response.Status)
            {
                case CheckStatus.Pass:
                    client.Agent.PassTTL(consulServiceSettings.Id, response.Note);
                    break;
                case CheckStatus.Warn:
                    client.Agent.WarnTTL(consulServiceSettings.Id, response.Note);
                    break;
                case CheckStatus.Fail:
                    client.Agent.FailTTL(consulServiceSettings.Id, response.Note);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}