using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using SuperGlue.Configuration;
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

                environment.RegisterTransient(typeof(IManageEventNumbersForProjections), (x, y) => new ManageEventNumbersForProjectionsInRavenDb(y.Resolve<IRavenSessions>().GetFor(y.GetSettings<RavenStreamManagerSettings>().DatabaseName)));

                return Task.CompletedTask;
            }, "superglue.RavenDb.Configure");
        }
    }
}