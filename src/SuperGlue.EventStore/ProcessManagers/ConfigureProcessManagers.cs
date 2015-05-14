using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ConfigureProcessManagers : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.ContainerSetup", environment => environment.Get<Action<Type>>("superglue.Container.RegisterAll")(typeof(IManageProcess)));
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}