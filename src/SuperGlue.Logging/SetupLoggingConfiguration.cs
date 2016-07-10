using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Logging
{
    public class SetupLoggingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.LoggingSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(ILog), typeof(ConsoleLogger)));

                environment[LogExtensions.LogConstants.WriteToLogFunction] = (Action<IDictionary<string, object>, Exception, string, string, object[]>)((env, ex, message, logLevel, parameters) =>
                {
                    var log = env.Resolve<ILog>();

                    log.Log(ex, message, logLevel, parameters);
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}