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
                var databaseName = ConfigurationManager.AppSettings["StreamManagers.RavenDb.DatabaseName"] ?? "";

                environment.RegisterTransient(typeof(IManageEventNumbersForProjections), (x, y) => new ManageEventNumbersForProjectionsInRavenDb(y.Resolve<IRavenSessions>().GetFor(databaseName)));

                return Task.CompletedTask;
            }, "superglue.RavenDb.Configure");
        }
    }
}