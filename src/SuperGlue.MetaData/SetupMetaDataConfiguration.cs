using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.MetaData
{
    public class SetupMetaDataConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.MetaDataSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Scan(typeof(ISupplyRequestMetaData)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}