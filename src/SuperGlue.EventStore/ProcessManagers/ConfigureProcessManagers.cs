using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ConfigureProcessManagers : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.ProcessManagers.Configured",
                environment =>
                {
                    environment.RegisterAll(typeof(IManageProcess));
                    environment.RegisterTransient(typeof(IProcessManagerInstaller), typeof(DefaultProcessManagerInstaller));

                    return Task.CompletedTask;
                }, "superglue.ContainerSetup");
        }
    }
}