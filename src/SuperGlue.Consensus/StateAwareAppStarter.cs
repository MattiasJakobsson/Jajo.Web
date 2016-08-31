using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Consensus
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StateAwareAppStarter
    {
        private readonly IStartApplication _startApplication;
        private readonly NodeRole? _requiredRole;
        private bool _started;

        public StateAwareAppStarter(IStartApplication startApplication, NodeRole? requiredRole)
        {
            _startApplication = startApplication;
            _requiredRole = requiredRole;
        }

        public string Chain => _startApplication.Chain;

        public async Task BringToState(AppFunc chain, IDictionary<string, object> settings, string environment, NodeRole role, IDictionary<string, string[]> hostArguments)
        {
            var shouldBeStarted = _requiredRole == null || _requiredRole == role;

            if (shouldBeStarted && !_started)
            {
                await _startApplication.Start(chain, settings, environment, hostArguments.ContainsKey(_startApplication.Name) ? hostArguments[_startApplication.Name] : new string[0]).ConfigureAwait(false);
                _started = true;
            }
            else if (!shouldBeStarted && _started)
            {
                await _startApplication.ShutDown(settings).ConfigureAwait(false);
                _started = false;
            }
        }

        public async Task ShutDown(IDictionary<string, object> settings)
        {
            if (!_started)
                return;

            await _startApplication.ShutDown(settings).ConfigureAwait(false);

            _started = false;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            return _startApplication.GetDefaultChain(buildApp, settings, environment);
        }
    }
}