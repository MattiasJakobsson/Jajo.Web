using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.CommandSender
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StartCommandSenderApplication : IStartApplication
    {
        public string Chain => "CommandSender";

        public Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            CommandPipeline.Use(chain);

            return Task.CompletedTask;
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            return buildApp
                .Use<ExecuteCurrentCommand>()
                .Build();
        }

        public NodeTypeRequirements SetupRequirements(NodeTypeRequirements nodeTypeRequirements)
        {
            return nodeTypeRequirements;
        }
    }
}