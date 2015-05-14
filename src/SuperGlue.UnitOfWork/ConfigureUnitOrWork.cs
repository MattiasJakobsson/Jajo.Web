using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.UnitOfWork
{
    public class ConfigureUnitOrWork : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.UnitOfWork.Configure", environment => environment.RegisterAll(typeof(ISuperGlueUnitOfWork)), "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }
    }
}