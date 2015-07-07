using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Document;

namespace SuperGlue.RavenDb
{
    public static class RavenDocumentStore
    {
        public static IDocumentStore Create(IDictionary<string, object> environment, string forDatabase = null)
        {
            var connectionStringName = environment.Get<string>(RavenEnvironmentConstants.ConnectionStringName) ?? "RavenDb";

            var documentStore = new DocumentStore
            {
                ConnectionStringName = connectionStringName
            };

            if (!string.IsNullOrEmpty(forDatabase))
                documentStore.DefaultDatabase = forDatabase;

            documentStore.Initialize();

            return documentStore;
        }
    }
}