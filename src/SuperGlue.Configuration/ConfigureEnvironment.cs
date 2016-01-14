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
                throw new ArgumentNullException(nameof(next));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            foreach (var item in _options.ApplicationEnvironment.Where(item => !environment.ContainsKey(item.Key)))
                environment[item.Key] = item.Value;

            var chainBefore = environment.GetCurrentChain();
            environment[ConfigurationsEnvironmentExtensions.ConfigurationConstants.CurrentChainData] = new ConfigurationsEnvironmentExtensions.ChainData(_options.ChainName, Guid.NewGuid().ToString());

            var correlationId = environment.GetCorrelationId();

            if (string.IsNullOrEmpty(correlationId))
            {
                using (environment.OpenCorrelationContext(Guid.NewGuid().ToString()))
                {
                    await _next(environment).ConfigureAwait(false);
                }
            }
            else
            {
                await _next(environment).ConfigureAwait(false);
            }

            environment[ConfigurationsEnvironmentExtensions.ConfigurationConstants.CurrentChainData] = chainBefore;
        }
    }

    public class ConfigureEnvironmentOptions
    {
        public ConfigureEnvironmentOptions(IDictionary<string, object> applicationEnvironment, string chainName)
        {
            ApplicationEnvironment = applicationEnvironment;
            ChainName = chainName;
        }

        public IDictionary<string, object> ApplicationEnvironment { get; }
        public string ChainName { get; }
    }
}