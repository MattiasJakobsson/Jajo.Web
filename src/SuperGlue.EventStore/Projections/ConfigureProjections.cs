using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Projections
{
    public class ConfigureProjections : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.Projections.Configured",
                environment =>
                {
                    environment.RegisterAll(typeof (IEventStoreProjection));
                    environment.RegisterTransient(typeof(IProjectionsInstaller), typeof(DefaultProjectionsInstaller));

                    return Task.CompletedTask;
                }, 
                "superglue.ContainerSetup", configureAction: x =>
                {
                    x.InitializeSettings<ProjectionSettings>();

                    return Task.CompletedTask;
                });
        }
    }
}