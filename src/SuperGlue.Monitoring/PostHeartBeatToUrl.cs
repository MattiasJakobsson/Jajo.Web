using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.HttpClient;

namespace SuperGlue.Monitoring
{
    public class PostHeartBeatToUrl : IMonitorHeartBeats
    {
        private readonly string _url;
        private readonly string _message;

        public PostHeartBeatToUrl(string url, string message = null)
        {
            _url = url;
            _message = message;
        }

        public async Task Beat(IDictionary<string, object> environment)
        {
            var httpClient = environment.Resolve<IHttpClient>();

            await httpClient
                .Start(new Uri(_url))
                .Method("POST")
                .Body(_message)
                .ThrowOnError()
                .Send()
                .ConfigureAwait(false);
        }
    }
}