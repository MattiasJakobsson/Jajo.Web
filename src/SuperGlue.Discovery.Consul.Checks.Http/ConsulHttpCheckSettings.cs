using System;
using System.Collections.Generic;

namespace SuperGlue.Discovery.Consul.Checks.Http
{
    public class ConsulHttpCheckSettings
    {
        public Func<IDictionary<string, object>, CheckResponse> Check { get; private set; }
        public string CheckEndpointRoute { get; private set; }
        public TimeSpan Interval { get; private set; }

        public ConsulHttpCheckSettings WithCheck(Func<IDictionary<string, object>, CheckResponse> check)
        {
            Check = check;

            return this;
        }

        public ConsulHttpCheckSettings WithRoute(string route)
        {
            CheckEndpointRoute = route;

            return this;
        }

        public ConsulHttpCheckSettings WithInterval(TimeSpan interval)
        {
            Interval = interval;

            return this;
        }

        public CheckResponse PerformCheck(IDictionary<string, object> dictionary)
        {
            return (Check ?? (x => new CheckResponse(CheckStatus.Pass)))(dictionary);
        }
    }
}