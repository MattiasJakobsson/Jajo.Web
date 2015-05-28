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

        public void Start(Func<IDictionary<string, object>, Task> chain, IDictionary<string, object> environment)
        {
            CommandPipeline.Use(chain);
        }

        public void ShutDown()
        {
            
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp)
        {
            return buildApp
                .Use<ExecuteCurrentCommand>()
                .Build();
        }
    }
}