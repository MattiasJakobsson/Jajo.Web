using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Topshelf
{
    public class SuperGlueServiceRuntime
    {
        private SuperGlueBootstrapper _bootstrapper;

        public void Start(string environment, params string[] excludedChains)
        {
            _bootstrapper = SuperGlueBootstrapper.Find();

            _bootstrapper.StartApplications(new Dictionary<string, object>(), environment, ApplicationStartersOverrides
                .Configure()
                .Chains(x =>
                {
                    foreach (var chain in excludedChains)
                        x.Exclude(chain);
                })).Wait();
        }

        public void Stop()
        {
            if(_bootstrapper != null)
                _bootstrapper.ShutDown().Wait();
        }
    }
}