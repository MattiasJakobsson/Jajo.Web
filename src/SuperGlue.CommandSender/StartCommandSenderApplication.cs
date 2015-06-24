using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.CommandSender
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StartCommandSenderApplication : IStartApplication
    {
        public string Chain
        {
            get { return "CommandSender"; }
        }

        public Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            return Task.Factory.StartNew(() => { CommandPipeline.Use(chain); });
        }

        public Task ShutDown()
        {
            return Task.Factory.StartNew(() => { });
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            return buildApp
                .Use<ExecuteCurrentCommand>()
                .Build();
        }
    }
}