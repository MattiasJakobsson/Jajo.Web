using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue;
using SuperGlue.Configuration;

namespace JaJo.Migrations.SuperGlue
{
    public class SetupMigrationsConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Jajo.Migrations", environment =>
            {
                environment.RegisterTransient(typeof(IMigrateApplication), typeof(SuperGlueMigrator));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}