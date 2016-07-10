using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;
using SuperGlue.EventStore.Projections;
using SuperGlue.RavenDb;

namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class SetupRavenDbStreamManagerConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.RavenDbStreamManagerSetup", environment =>
            {
                environment.AlterSettings<RavenStreamManagerSettings>(x =>
                {
                    if (string.IsNullOrEmpty(x.DatabaseName))
                        x.UsingDatabase(environment.Resolve<IApplicationConfiguration>().GetSetting("StreamManagers.RavenDb.DatabaseName") ?? "");
                });

                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IManageEventNumbersForProjections), 
                    (y, z) => new ManageEventNumbersForProjectionsInRavenDb(z.Resolve<IRavenSessions>().GetFor(environment.GetSettings<RavenStreamManagerSettings>().DatabaseName))));

                return Task.CompletedTask;
            }, "superglue.RavenDb.Configure");
        }
    }
}