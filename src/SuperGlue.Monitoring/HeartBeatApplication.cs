using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Monitoring
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HeartBeatApplication : IStartApplication
    {
        private readonly ICollection<HandleHeartBeat> _handleHeartBeats = new List<HandleHeartBeat>();

        public string Chain => "chains.Monitoring.HeartBeat";

        public Task Start(Func<IDictionary<string, object>, Task> chain, IDictionary<string, object> settings, string environment)
        {
            foreach (var heartBeatUrl in (settings.GetSettings<HeartBeatSettings>() ?? new HeartBeatSettings()).GetHeartBeatUrls())
                _handleHeartBeats.Add(new HandleHeartBeat(chain, heartBeatUrl.Item1, heartBeatUrl.Item2));

            foreach (var handleHeartBeat in _handleHeartBeats)
                handleHeartBeat.Start(settings);

            return Task.CompletedTask;
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            foreach (var handleHeartBeat in _handleHeartBeats)
                handleHeartBeat.Stop();

            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            return buildApp
                .Use<InvokeHeartBeat>()
                .Build();
        }
    }
}