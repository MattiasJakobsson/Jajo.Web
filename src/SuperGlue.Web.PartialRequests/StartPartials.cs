using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.PartialRequests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StartPartials : IStartApplication
    {
        public string Chain { get { return "chains.Partials"; } }

        public Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            return Task.Factory.StartNew(() => Partials.Initialize(chain));
        }

        public Task ShutDown()
        {
            return Task.Factory.StartNew(() => { });
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            return null;
        }
    }
}