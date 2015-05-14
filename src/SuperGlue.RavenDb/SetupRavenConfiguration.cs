using System;
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
                var registerTransient = environment.Get<Action<Type, Type>>("superglue.Container.RegisterTransient");
                var registerSingletone = environment.Get<Action<Type, object>>("superglue.Container.RegisterSingleton");
                var registerAllClosing = environment.Get<Action<Type>>("superglue.Container.RegisterAllClosing");

                var connectionStringName = environment.Get<string>("ravendb.ConnectionStringName") ?? "RavenDb";
                var documentStore = CreateDocumentStore(connectionStringName);

                registerSingletone(typeof(IDocumentStore), documentStore);
                registerTransient(typeof(IRavenSearch), typeof(DefaultRavenSearch));

                registerAllClosing(typeof(IRavenSearchPart<>));
                registerAllClosing(typeof(IRavenSpecialCommandSearch<>));
                registerAllClosing(typeof(IHandleLeftoverSearchPart<>));
                registerAllClosing(typeof(IRavenFreeTextSearch<>));
            });
        }

        public void Shutdown(IDictionary<string, object> applicationData)
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