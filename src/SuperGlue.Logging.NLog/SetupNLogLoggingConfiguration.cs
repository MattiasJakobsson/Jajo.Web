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
            yield return new ConfigurationSetupResult("superglue.Nlog.LoggingSetup", environment =>
            {
                environment.RegisterTransient(typeof(ILog), (x, y) => new NLogLogger(LogManager.GetLogger(x.FullName)));

                return Task.CompletedTask;
            }, "superglue.LoggingSetup");
        }
    }
}