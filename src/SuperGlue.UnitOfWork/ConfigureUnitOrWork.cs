using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.UnitOfWork
{
    public class ConfigureUnitOrWork : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironmente)
        {
            yield return new ConfigurationSetupResult("superglue.UnitOfWork.Configure", environment => environment.RegisterAll(typeof(ISuperGlueUnitOfWork)), "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}