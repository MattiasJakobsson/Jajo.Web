using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Logging
{
    public class SetupLoggingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.LoggingSetup", environment =>
            {
                environment.RegisterTransient(typeof(ILog), typeof(ConsoleLogger));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}