using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class ApplicationStartersOverrides
    {
        private readonly List<Type> _preferedApplicationStarters = new List<Type>();
        private readonly List<string> _excludedChains = new List<string>();

        private ApplicationStartersOverrides()
        {
            
        }

        public static ApplicationStartersOverrides Configure()
        {
            return new ApplicationStartersOverrides();
        }

        public ApplicationStartersOverrides AppStarters(Action<AppStarterConfiguration> configure)
        {
            var config = new AppStarterConfiguration(this);

            configure(config);

            return this;
        }

        public ApplicationStartersOverrides Chains(Action<ChainsConfiguration> configure)
        {
            var config = new ChainsConfiguration(this);

            configure(config);

            return this;
        }

        internal int GetSortOrder(IStartApplication applicationStarter)
        {
            return _preferedApplicationStarters.Contains(applicationStarter.GetType()) ? 0 : 1;
        }

        internal bool ShouldStart(string chain)
        {
            return !_excludedChains.Contains(chain);
        }

        public class AppStarterConfiguration
        {
            private readonly ApplicationStartersOverrides _applicationStartersOverrides;

            public AppStarterConfiguration(ApplicationStartersOverrides applicationStartersOverrides)
            {
                _applicationStartersOverrides = applicationStartersOverrides;
            }

            public AppStarterConfiguration Prefere<TApplicationStarter>() where TApplicationStarter : IStartApplication
            {
                _applicationStartersOverrides._preferedApplicationStarters.Add(typeof(TApplicationStarter));

                return this;
            }
        }

        public class ChainsConfiguration
        {
            private readonly ApplicationStartersOverrides _applicationStartersOverrides;

            public ChainsConfiguration(ApplicationStartersOverrides applicationStartersOverrides)
            {
                _applicationStartersOverrides = applicationStartersOverrides;
            }

            public ChainsConfiguration Exclude(string chain)
            {
                _applicationStartersOverrides._excludedChains.Add(chain);

                return this;
            }
        }
    }
}