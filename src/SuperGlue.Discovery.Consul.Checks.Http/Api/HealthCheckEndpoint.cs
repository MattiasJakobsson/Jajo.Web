using System.Collections.Generic;
using SuperGlue.Configuration;
using SuperGlue.Web;

namespace SuperGlue.Discovery.Consul.Checks.Http.Api
{
    public class HealthCheckEndpoint
    {
        private readonly IDictionary<string, object> _environment;
        private readonly ConsulHttpCheckSettings _consulHttpCheckSettings;

        public HealthCheckEndpoint(IDictionary<string, object> environment)
        {
            _environment = environment;
            _consulHttpCheckSettings = environment.GetSettings<ConsulHttpCheckSettings>();
        }

        public string Check(HealthCheckInput input)
        {
            var response = _consulHttpCheckSettings.PerformCheck(_environment);

            var note = response.Note;

            switch (response.Status)
            {
                case CheckStatus.Pass:
                    _environment.GetResponse().StatusCode = 200;

                    if (string.IsNullOrEmpty(note))
                        note = "Pass";

                    break;
                case CheckStatus.Warn:
                    _environment.GetResponse().StatusCode = 429;

                    if (string.IsNullOrEmpty(note))
                        note = "Warn";

                    break;
                case CheckStatus.Fail:
                    _environment.GetResponse().StatusCode = 500;

                    if (string.IsNullOrEmpty(note))
                        note = "Fail";

                    break;
            }

            return note;
        }
    }

    public class HealthCheckInput
    {
         
    }
}