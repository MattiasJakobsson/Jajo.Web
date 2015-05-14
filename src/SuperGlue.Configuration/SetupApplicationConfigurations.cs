using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class SetupApplicationConfigurations : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.Configuration.ApplicationsConfigured", environment =>
            {
                var registerAll = environment.Get<Action<Type>>("superglue.Container.RegisterAll");

                registerAll(typeof (IStartApplication));
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}