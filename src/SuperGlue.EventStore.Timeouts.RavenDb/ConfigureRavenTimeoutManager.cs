using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Document;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Timeouts.RavenDb
{
    public class ConfigureRavenTimeoutManager : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.TimeoutManager.Configure", environment =>
            {
                var timeOutManagerName = environment.Get<string>(RavenTimeoutManagerEnvironmentConstants.TimeoutManagerName);
                var connectionStringName = environment.Get<string>(RavenTimeoutManagerEnvironmentConstants.ConnectionStringName) ?? "RavenDb";
                var databaseName = environment.Get<string>(RavenTimeoutManagerEnvironmentConstants.TimeoutDatabaseName);

                var documentStore = new DocumentStore
                {
                    ConnectionStringName = connectionStringName
                };

                if (!string.IsNullOrEmpty(databaseName))
                    documentStore.DefaultDatabase = databaseName;

                documentStore.Initialize();

                new RavenTimeOutDataIndex().Execute(documentStore);

                TimeOutManager.Configure(() => new StoreTimeOutsInRavenDb(documentStore, timeOutManagerName, databaseName));

                return Task.CompletedTask;
            }, "superglue.RavenDb.Configure");
        }
    }
}