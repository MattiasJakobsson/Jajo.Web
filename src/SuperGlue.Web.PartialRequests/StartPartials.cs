using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.PartialRequests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StartPartials : IStartApplication
    {
        public string Chain => "chains.Partials";

        public Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            Partials.Initialize(chain);

            return Task.CompletedTask;
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            return null;
        }

        public NodeTypeRequirements SetupRequirements(NodeTypeRequirements nodeTypeRequirements)
        {
            return nodeTypeRequirements;
        }
    }
}