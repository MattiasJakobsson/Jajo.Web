using System.Collections.Generic;
using System.Configuration;
using System.Web;
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
            if (isConfigured)
                return;

            var settings = new Dictionary<string, object>
            {
                {SystemWebEnvironmentConstants.AppBuilder, app}
            };

            var excludedChains = (ConfigurationManager.AppSettings["SuperGlue.Chains.Excluded"] ?? "").Split(';');

            var bootstrapper = SuperGlueBootstrapper.Find();

            bootstrapper.StartApplications(settings, HttpContext.Current.IsDebuggingEnabled ? "local" : "production", ApplicationStartersOverrides
                .Configure()
                .AppStarters(x => x.Prefere<StartSystemWebHost>())
                .Chains(x =>
                {
                    foreach (var chain in excludedChains)
                        x.Exclude(chain);
                })).Wait();

            isConfigured = true;
        }
    }
}
