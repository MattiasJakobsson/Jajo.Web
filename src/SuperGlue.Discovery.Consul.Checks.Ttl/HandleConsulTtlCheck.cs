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
            var note = response.Note;

            switch (response.Status)
            {
                case CheckStatus.Pass:
                    if (string.IsNullOrEmpty(note))
                        note = "Pass";

                    await client.Agent.PassTTL($"service:{consulServiceSettings.Id}", note).ConfigureAwait(false);

                    break;
                case CheckStatus.Warn:
                    if (string.IsNullOrEmpty(note))
                        note = "Warn";

                    await client.Agent.WarnTTL($"service:{consulServiceSettings.Id}", note).ConfigureAwait(false);

                    break;
                case CheckStatus.Fail:
                    if (string.IsNullOrEmpty(note))
                        note = "Fail";

                    await client.Agent.FailTTL($"service:{consulServiceSettings.Id}", note).ConfigureAwait(false);

                    break;
            }
        }
    }
}