using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;
using SuperGlue.Configuration;
using SuperGlue.RavenDb.Search;

namespace SuperGlue.RavenDb
{
    public class SetupRavenConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.RavenDb.Configure", environment =>
            {
                var documentStore = RavenDocumentStore.Create(environment);

                environment[RavenEnvironmentConstants.DocumentStore] = documentStore;

                environment.RegisterSingleton(typeof(IDocumentStore), documentStore);
                environment.RegisterTransient(typeof(IRavenSearch), typeof(DefaultRavenSearch));
                environment.RegisterTransient(typeof(IRavenSessions), typeof(DefaultRavenSessions));

                environment.RegisterAllClosing(typeof(IRavenSearchPart<>));
                environment.RegisterAllClosing(typeof(IRavenSpecialCommandSearch<>));
                environment.RegisterAllClosing(typeof(IHandleLeftoverSearchPart<>));
                environment.RegisterAllClosing(typeof(IRavenFreeTextSearch<>));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}