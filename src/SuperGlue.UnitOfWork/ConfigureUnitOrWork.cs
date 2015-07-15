using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.UnitOfWork
{
    public class ConfigureUnitOrWork : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironmente)
        {
            yield return new ConfigurationSetupResult("superglue.UnitOfWork.Configure", environment =>
            {
                environment.RegisterAll(typeof(ISuperGlueUnitOfWork));
                environment.RegisterAll(typeof(IApplicationTask));

                environment.SubscribeTo(ConfigurationEvents.BeforeApplicationStart, async x =>
                {
                    var applicationTasks = x.ResolveAll<IApplicationTask>();

                    foreach (var applicationTask in applicationTasks)
                        await applicationTask.Start();
                });

                environment.SubscribeTo(ConfigurationEvents.AfterApplicationShutDown, async x =>
                {
                    var applicationTasks = x.ResolveAll<IApplicationTask>();

                    foreach (var applicationTask in applicationTasks)
                        await applicationTask.ShutDown();
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}