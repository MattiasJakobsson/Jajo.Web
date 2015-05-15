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
        private static bool isConfigured;

        public void Configuration(IAppBuilder app)
        {
            if(isConfigured)
                return;

            var settings = new Dictionary<string, object>
            {
                {SystemWebEnvironmentConstants.AppBuilder, app}
            };

            var bootstrapper = SuperGlueBootstrapper.Find();

            bootstrapper.StartApplications(settings);

            isConfigured = true;
        }
    }
}
