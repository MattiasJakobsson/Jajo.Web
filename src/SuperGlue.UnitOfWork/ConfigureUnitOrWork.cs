using System;
using System.Collections.Generic;
using System.Linq;
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
                    var applicationTasks = x.ResolveAll<IApplicationTask>().ToList();
                    var unitOfWorks = x.ResolveAll<ISuperGlueUnitOfWork>().ToList();

                    try
                    {
                        foreach (var unitOfWork in unitOfWorks)
                            await unitOfWork.Begin();

                        foreach (var applicationTask in applicationTasks)
                            await applicationTask.Start();

                        foreach (var unitOfWork in unitOfWorks)
                            await unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        foreach (var applicationTask in applicationTasks)
                            await applicationTask.Exception(ex);

                        foreach (var unitOfWork in unitOfWorks)
                            await unitOfWork.Rollback(ex);

                        throw;
                    }
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