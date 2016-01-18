using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ConfiguredAppStarter
    {
        private readonly IStartApplication _starter;

        public ConfiguredAppStarter(IStartApplication starter)
        {
            Requirements = starter.SetupRequirements(new NodeTypeRequirements());
            _starter = starter;
        }

        public NodeTypeRequirements Requirements { get; set; }
        public bool Started { get; private set; }
        public string Chain => _starter.Chain;

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            if (Started)
                return;

            await _starter.Start(chain, settings, environment).ConfigureAwait(false);
            Started = true;
        }

        public async Task ShutDown(IDictionary<string, object> settings)
        {
            if (!Started)
                return;

            await _starter.ShutDown(settings).ConfigureAwait(false);
            Started = false;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            return _starter.GetDefaultChain(buildApp, settings, environment);
        }
    }
}