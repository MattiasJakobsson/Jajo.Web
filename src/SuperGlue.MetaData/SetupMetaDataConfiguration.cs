using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.MetaData
{
    public class SetupMetaDataConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.MetaDataSetup", environment =>
            {
                environment.RegisterAll(typeof(ISupplyRequestMetaData));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}