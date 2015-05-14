using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Endpoints
{
    public class ConfigureEndpoints : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.UnitOfWork.Configure", environment => environment.Get<Action<Type>>("superglue.Container.RegisterAllClosing")(typeof(IExecuteTypeOfEndpoint<>)), "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}