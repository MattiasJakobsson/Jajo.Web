using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Logging.NLog
{
    public class SetupNLogLoggingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Nlog.LoggingSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(ILog), (y, z) => new NLogLogger(LogManager.GetLogger(y?.FullName ?? ""))));

                return Task.CompletedTask;
            }, "superglue.LoggingSetup");
        }
    }
}