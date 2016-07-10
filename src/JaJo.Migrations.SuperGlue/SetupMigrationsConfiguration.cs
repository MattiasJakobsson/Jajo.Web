using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace JaJo.Migrations.SuperGlue
{
    public class SetupMigrationsConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Jajo.Migrations", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IMigrateApplication), typeof(SuperGlueMigrator)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}