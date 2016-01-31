using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Monitoring;

namespace SuperGlue.Discovery.Consul.Checks.Ttl
{
    public class HandleConsulTtlCheck : IMonitorHeartBeats
    {
        public async Task Beat(IDictionary<string, object> environment)
        {
            var consulServiceSettings = environment.GetSettings<ConsulServiceSettings>();
            var checkSettings = environment.GetSettings<ConsulTtlCheckSettings>();

            var client = consulServiceSettings.CreateClient();

            var response = checkSettings.PerformCheck(environment);

            switch (response.Status)
            {
                case CheckStatus.Pass:
                    await client.Agent.PassTTL(consulServiceSettings.Id, response.Note).ConfigureAwait(false);
                    break;
                case CheckStatus.Warn:
                    await client.Agent.WarnTTL(consulServiceSettings.Id, response.Note).ConfigureAwait(false);
                    break;
                case CheckStatus.Fail:
                    await client.Agent.FailTTL(consulServiceSettings.Id, response.Note).ConfigureAwait(false);
                    break;
            }
        }
    }
}