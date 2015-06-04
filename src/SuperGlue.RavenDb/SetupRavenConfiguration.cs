using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Document;
using SuperGlue.Configuration;
using SuperGlue.RavenDb.Search;

namespace SuperGlue.RavenDb
{
    public class SetupRavenConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.RavenDb.Configure", environment =>
            {
                var connectionStringName = environment.Get<string>(RavenEnvironmentConstants.ConnectionStringName) ?? "RavenDb";
                var documentStore = CreateDocumentStore(connectionStringName);

                environment[RavenEnvironmentConstants.DocumentStore] = documentStore;

                environment.RegisterSingleton(typeof(IDocumentStore), documentStore);
                environment.RegisterTransient(typeof(IRavenSearch), typeof(DefaultRavenSearch));

                environment.RegisterAllClosing(typeof(IRavenSearchPart<>));
                environment.RegisterAllClosing(typeof(IRavenSpecialCommandSearch<>));
                environment.RegisterAllClosing(typeof(IHandleLeftoverSearchPart<>));
                environment.RegisterAllClosing(typeof(IRavenFreeTextSearch<>));
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }

        public void Configure(SettingsConfiguration configuration)
        {
            
        }

        private static IDocumentStore CreateDocumentStore(string connectionStringName)
        {
            var documentStore = new DocumentStore
            {
                ConnectionStringName = connectionStringName
            };

            documentStore.Initialize();

            return documentStore;
        }
    }
}