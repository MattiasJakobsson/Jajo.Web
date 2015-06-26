using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.UnitOfWork
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StartSetupApplication : IStartApplication
    {
        private static AppFunc setupChain;

        public string Chain
        {
            get { return "chains.Setup"; }
        }

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            setupChain = chain;

            if (setupChain == null)
                return;

            settings[SetupEnvironmentExtensions.SetupConstants.SetupEnvironmentMode] = SetupMode.StartUp;

            await setupChain(settings);

            settings[SetupEnvironmentExtensions.SetupConstants.SetupEnvironmentMode] = SetupMode.None;
        }

        public async Task ShutDown(IDictionary<string, object> settings)
        {
            if (setupChain == null)
                return;

            settings[SetupEnvironmentExtensions.SetupConstants.SetupEnvironmentMode] = SetupMode.ShutDown;

            await setupChain(settings);

            settings[SetupEnvironmentExtensions.SetupConstants.SetupEnvironmentMode] = SetupMode.None;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            return buildApp
                .Use<HandleUnitOfWork>()
                .Use<HandleApplicationTasks>()
                .Build();
        }
    }
}