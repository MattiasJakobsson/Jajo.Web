using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.EventStore.Projections
{
    public class ConfigureProjections : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.Projections.Configured", environment =>
                {
                    environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IProjectionsInstaller), typeof(DefaultProjectionsInstaller))
                        .Scan(typeof(IEventStoreProjection)));

                    return Task.CompletedTask;
                }, 
                "superglue.ContainerSetup");
        }
    }
}