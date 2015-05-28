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

        public void Start(AppFunc chain, IDictionary<string, object> environment)
        {
            Partials.Initialize(chain);
        }

        public void ShutDown()
        {
            
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp)
        {
            return null;
        }
    }
}