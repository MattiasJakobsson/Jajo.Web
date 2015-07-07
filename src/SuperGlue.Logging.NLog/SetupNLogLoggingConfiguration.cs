using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using SuperGlue.Configuration;

namespace SuperGlue.Logging.NLog
{
    public class SetupNLogLoggingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.LoggingSetup", environment =>
            {
                environment.RegisterTransient(typeof(ILog), (x, y) => new NLogLogger(LogManager.GetLogger(x.FullName)));
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.CompletedTask;
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.CompletedTask;
        }
    }
}