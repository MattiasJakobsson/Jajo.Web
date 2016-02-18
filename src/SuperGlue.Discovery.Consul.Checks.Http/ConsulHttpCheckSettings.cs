using System;
using System.Collections.Generic;

namespace SuperGlue.Discovery.Consul.Checks.Http
{
    public class ConsulHttpCheckSettings
    {
        public Func<IDictionary<string, object>, CheckResponse> Check { get; private set; }
        public CheckUrl CheckEndpoint { get; private set; }
        public TimeSpan Interval { get; private set; }

        public ConsulHttpCheckSettings WithCheck(Func<IDictionary<string, object>, CheckResponse> check)
        {
            Check = check;

            return this;
        }

        public ConsulHttpCheckSettings WithRoute(object routedTo, string urlPart)
        {
            CheckEndpoint = new CheckUrl(urlPart, routedTo);

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

        public class CheckUrl
        {
            public CheckUrl(string urlPath, object input)
            {
                UrlPath = urlPath;
                Input = input;
            }

            public string UrlPath { get; private set; }
            public object Input { get; private set; } 
        }
    }
}