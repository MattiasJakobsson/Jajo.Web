using System.Collections.Generic;
using SuperGlue;
using SuperGlue.Configuration;
using SuperGlue.RavenDb;

namespace JaJo.Migrations.Storage.SuperGlue.RavenDb
{
    public class SetupRavenDbMigrationsStorage : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Jajo.Migrations.RavenDbStorage", async environment =>
            {
                environment.RegisterTransient(typeof(IMigrationStorage), typeof(RavenMigrationStorage));

                await environment.Resolve<IRavenSessions>().EnsureDbExists((environment.GetSettings<MigrationRavenStorageSettings>() ?? new MigrationRavenStorageSettings()).GetDatabase());
            }, "superglue.RavenDb.Configure");
        }
    }
}