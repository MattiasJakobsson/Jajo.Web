using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperGlue.Monitoring
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleHeartBeat
    {
        private readonly CancellationTokenSource _tokenSource;
        private readonly AppFunc _chain;
        private readonly IMonitorHeartBeats _monitorHeartBeats;
        private readonly TimeSpan _interval;

        public HandleHeartBeat(AppFunc chain, IMonitorHeartBeats monitorHeartBeats, TimeSpan interval)
        {
            _chain = chain;
            _monitorHeartBeats = monitorHeartBeats;
            _interval = interval;
            _tokenSource = new CancellationTokenSource();
        }

        public void Start(IDictionary<string, object> environment)
        {
            StartBeating(environment);
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
        }

        private void StartBeating(IDictionary<string, object> environment)
        {
            var token = _tokenSource.Token;

            Task.Factory
                .StartNew(async x => await Beat(x, environment), token, TaskCreationOptions.LongRunning)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);

                    StartBeating(environment);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task Beat(object obj, IDictionary<string, object> environment)
        {
            var cancellationToken = (CancellationToken)obj;

            while (!cancellationToken.IsCancellationRequested)
            {
                var requestEnvironment = new Dictionary<string, object>();

                foreach (var item in environment)
                    requestEnvironment[item.Key] = item.Value;

                requestEnvironment.SetHeartBeatMonitor(_monitorHeartBeats);

                await _chain(requestEnvironment);

                Thread.Sleep(_interval);
            }
        }
    }
}