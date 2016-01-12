using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Web;

namespace SuperGlue.Discovery.Consul.Checks.Http.Api
{
    public class HealthCheckEndpoint
    {
        private readonly IDictionary<string, object> _environment;
        private readonly ConsulHttpCheckSettings _consulHttpCheckSettings;

        public HealthCheckEndpoint(IDictionary<string, object> environment, ConsulHttpCheckSettings consulHttpCheckSettings)
        {
            _environment = environment;
            _consulHttpCheckSettings = consulHttpCheckSettings;
        }

        public string Check()
        {
            var response = _consulHttpCheckSettings.Check(_environment);

            switch (response.Status)
            {
                case CheckStatus.Pass:
                    _environment.GetResponse().StatusCode = 200;
                    break;
                case CheckStatus.Warn:
                    _environment.GetResponse().StatusCode = 429;
                    break;
                case CheckStatus.Fail:
                    _environment.GetResponse().StatusCode = 500;
                    break;
            }

            return response.Note;
        }
    }
}