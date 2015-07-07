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
                var databaseName = ConfigurationManager.AppSettings["StreamManagers.RavenDb.DatabaseName"];
                
                if (string.IsNullOrEmpty(databaseName))
                    databaseName = "Site";

                environment.RegisterTransient(typeof(IManageProcessManagerStreamEventNumbers), (x, y) => new ManageProcessManagerStreamEventNumbersInRavenDb(y.Resolve<IRavenSessions>().GetFor(databaseName)));
                environment.RegisterTransient(typeof(IManageEventNumbersForProjections), (x, y) => new ManageEventNumbersForProjectionsInRavenDb(y.Resolve<IRavenSessions>().GetFor(databaseName)));
                environment.RegisterTransient(typeof(IManageEventNumbersForSubscriber), (x, y) => new ManageEventNumbersForSubscriberInRavenDb(y.Resolve<IRavenSessions>().GetFor(databaseName)));
            }, "superglue.RavenDb.Configure");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.CompletedTask;
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.CompletedTask;
        }
    }
}