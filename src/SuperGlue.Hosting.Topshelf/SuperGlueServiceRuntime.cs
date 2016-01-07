using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Topshelf
{
    public class SuperGlueServiceRuntime
    {
        private SuperGlueBootstrapper _bootstrapper;

        public Task Start(string environment, int retryCount, TimeSpan? retryInterval, params string[] excludedChains)
        {
            _bootstrapper = SuperGlueBootstrapper.Find();

            return _bootstrapper.StartApplications(new Dictionary<string, object>(), environment, ApplicationStartersOverrides
                .Configure()
                .Chains(x =>
                {
                    foreach (var chain in excludedChains)
                        x.Exclude(chain);
                }), retryCount, retryInterval);
        }

        public Task Stop()
        {
            if(_bootstrapper != null)
                return _bootstrapper.ShutDown();

            return Task.CompletedTask;
        }
    }
}