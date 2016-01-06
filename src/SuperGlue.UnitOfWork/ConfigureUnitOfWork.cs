using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.UnitOfWork
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ConfigureUnitOfWork : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironmente)
        {
            yield return new ConfigurationSetupResult("superglue.UnitOfWork.Configure", environment =>
            {
                environment.RegisterAll(typeof(ISuperGlueUnitOfWork));
                environment.RegisterAll(typeof(IApplicationTask));

                environment.SubscribeTo(ConfigurationEvents.BeforeApplicationStart, async x =>
                {
                    var requestEnvironment = new Dictionary<string, object>();

                    foreach (var item in x)
                        requestEnvironment[item.Key] = item.Value;

                    var startupChain = await GetApplicationStartupChain(x);

                    await startupChain(requestEnvironment);
                });

                environment.SubscribeTo(ConfigurationEvents.AfterApplicationShutDown, async x =>
                {
                    var requestEnvironment = new Dictionary<string, object>();

                    foreach (var item in x)
                        requestEnvironment[item.Key] = item.Value;

                    var shutdownChain = await GetApplicationShutdownChain(x);

                    await shutdownChain(requestEnvironment);
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }

        private static Task<AppFunc> GetApplicationStartupChain(IDictionary<string, object> environment)
        {
            return environment.GetNamedChain("chains.StartupApplication", x => x.Use<HandleUnitOfWork>().Use<ExecuteStartup>());
        }

        private static Task<AppFunc> GetApplicationShutdownChain(IDictionary<string, object> environment)
        {
            return environment.GetNamedChain("chains.ShutdownApplication", x => x.Use<HandleUnitOfWork>().Use<ExecuteShutdown>());
        }
    }
}