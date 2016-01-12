using System;
using System.Collections.Generic;

namespace SuperGlue.Discovery.Consul.Checks.Ttl
{
    public class ConsulTtlCheckSettings
    {
        public TimeSpan Ttl { get; private set; }
        public TimeSpan RequestInterval { get; private set; }
        public Func<IDictionary<string, object>, CheckResponse> Check { get; private set; }

        public ConsulTtlCheckSettings WithTtl(TimeSpan ttl)
        {
            Ttl = ttl;

            return this;
        }

        public ConsulTtlCheckSettings WithRequestInterval(TimeSpan interval)
        {
            RequestInterval = interval;

            return this;
        }

        public ConsulTtlCheckSettings WithCheck(Func<IDictionary<string, object>, CheckResponse> check)
        {
            Check = check;

            return this;
        }

        public CheckResponse PerformCheck(IDictionary<string, object> dictionary)
        {
            return (Check ?? (x => new CheckResponse(CheckStatus.Pass)))(dictionary);
        }
    }
}