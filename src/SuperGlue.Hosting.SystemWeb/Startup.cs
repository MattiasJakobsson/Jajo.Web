using System.Collections.Generic;
using Microsoft.Owin;
using Owin;
using SuperGlue.Configuration;
using SuperGlue.Hosting.SystemWeb;

[assembly: OwinStartup(typeof(Startup))]
namespace SuperGlue.Hosting.SystemWeb
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var settings = new Dictionary<string, object>
            {
                {"superglue.Hosting.AppBuilder", app}
            };

            var bootstrapper = SuperGlueBootstrapper.Find();

            bootstrapper.StartApplications(settings);
        }
    }
}
