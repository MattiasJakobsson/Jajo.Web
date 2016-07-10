using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ConfigureProcessManagers : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.ProcessManagers.Configured", environment =>
                {
                    environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IProcessManagerInstaller), typeof(DefaultProcessManagerInstaller))
                        .Scan(typeof(IManageProcess)));

                    return Task.CompletedTask;
                }, "superglue.ContainerSetup");
        }
    }
}