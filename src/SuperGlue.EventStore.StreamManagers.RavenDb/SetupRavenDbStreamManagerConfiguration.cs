using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.EventStore.ProcessManagers;
using SuperGlue.EventStore.Projections;
using SuperGlue.EventStore.Subscribers;
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

                environment.RegisterTransient(typeof(IManageProcessManagerStreamEventNumbers), (x, y) => new ManageProcessManagerStreamEventNumbersInRavenDb(y.Resolve<IRavenSessions>().GetFor(databaseName)));
                environment.RegisterTransient(typeof(IManageEventNumbersForProjections), (x, y) => new ManageEventNumbersForProjectionsInRavenDb(y.Resolve<IRavenSessions>().GetFor(databaseName)));
                environment.RegisterTransient(typeof(IManageEventNumbersForSubscriber), (x, y) => new ManageEventNumbersForSubscriberInRavenDb(y.Resolve<IRavenSessions>().GetFor(databaseName)));

                return Task.CompletedTask;
            }, "superglue.RavenDb.Configure");
        }
    }
}