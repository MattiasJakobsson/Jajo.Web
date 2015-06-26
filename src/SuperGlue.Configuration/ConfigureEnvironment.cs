using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ConfigureEnvironment
    {
        private readonly AppFunc _next;
        private readonly ConfigureEnvironmentOptions _options;

        public ConfigureEnvironment(AppFunc next, ConfigureEnvironmentOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (options == null)
                throw new ArgumentNullException("options");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            foreach (var item in _options.ApplicationEnvironment.Where(item => !environment.ContainsKey(item.Key)))
                environment[item.Key] = item.Value;

            var chainBefore = environment.GetCurrentChain();
            environment[ConfigurationsEnvironmentExtensions.ConfigurationConstants.ChainName] = _options.ChainName;

            await _next(environment);

            environment[ConfigurationsEnvironmentExtensions.ConfigurationConstants.ChainName] = chainBefore;
        }
    }

    public class ConfigureEnvironmentOptions
    {
        public ConfigureEnvironmentOptions(IDictionary<string, object> applicationEnvironment, string chainName)
        {
            ApplicationEnvironment = applicationEnvironment;
            ChainName = chainName;
        }

        public IDictionary<string, object> ApplicationEnvironment { get; private set; }
        public string ChainName { get; private set; }
    }
}