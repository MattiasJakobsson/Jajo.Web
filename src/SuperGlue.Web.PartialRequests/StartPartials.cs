using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.PartialRequests
{
    public class StartPartials : IStartApplication
    {
        public void Start(IDictionary<string, Func<IDictionary<string, object>, Task>> chains, IDictionary<string, object> environment)
        {
            if(!chains.ContainsKey("chains.Partials"))
                return;

            var chain = chains["chains.Partials"];

            Partials.Initialize(chain);
        }

        public void ShutDown()
        {
            
        }
    }
}