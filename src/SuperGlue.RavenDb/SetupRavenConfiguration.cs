using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;
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

                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IDocumentStore), documentStore)
                    .Register(typeof(IRavenSearch), typeof(DefaultRavenSearch))
                    .Register(typeof(IRavenSessions), typeof(DefaultRavenSessions))
                    .ScanOpenType(typeof(IRavenSearchPart<>))
                    .ScanOpenType(typeof(IRavenSpecialCommandSearch<>))
                    .ScanOpenType(typeof(IHandleLeftoverSearchPart<>))
                    .ScanOpenType(typeof(IRavenFreeTextSearch<>)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}