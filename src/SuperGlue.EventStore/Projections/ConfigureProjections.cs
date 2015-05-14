using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Projections
{
    public class ConfigureProjections : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.ContainerSetup", environment => environment.Get<Action<Type>>("superglue.Container.RegisterAll")(typeof(IEventStoreProjection)));
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}