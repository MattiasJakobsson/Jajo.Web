using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Document;
using SuperGlue.Web;
using SuperGlue.Web.Configuration;

namespace SuperGlue.EventStore.Timeouts.RavenDb
{
    public class ConfigureRavenTimeoutManager : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.TimeoutManager.Configure", environment =>
            {
                var timeOutManagerName = environment.Get<string>("timeoutmanager.Name");
                var connectionStringName = environment.Get<string>("ravendb.ConnectionStringName") ?? "RavenDb";
                var databaseName = environment.Get<string>("ravendb.TimeoutManager.Database");

                var defaultDocumentStore = environment.Get<IDocumentStore>("superglue.RavenDb.DocumentStore");

                if (!string.IsNullOrEmpty(databaseName))
                    defaultDocumentStore.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists(databaseName);

                var documentStore = new DocumentStore
                {
                    ConnectionStringName = connectionStringName,
                    DefaultDatabase = string.IsNullOrEmpty(databaseName) ? ((DocumentStore)defaultDocumentStore).DefaultDatabase : databaseName
                };

                documentStore.Initialize();

                new RavenTimeOutDataIndex().Execute(documentStore);

                TimeOutManager.Configure(() => new StoreTimeOutsInRavenDb(defaultDocumentStore, timeOutManagerName, databaseName));
            }, "superglue.RavenDb.Configure");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}