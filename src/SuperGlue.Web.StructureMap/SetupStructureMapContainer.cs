using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using StructureMap;
using SuperGlue.Web.Configuration;

namespace SuperGlue.Web.StructureMap
{
    public class SetupStructureMapContainer : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            var container = new Container();

            yield return new ConfigurationSetupResult("superglue.StructureMap.ContainerSetup", x => x["superglue.StructureMap.Container"] = container);
            yield return new ConfigurationSetupResult("superglue.ContainerSetup", environment =>
            {
                var currentContainer = environment.GetContainer();

                environment["superglue.ResolveInstance"] = (Func<Type, object>)(x =>
                {
                    try
                    {
                        return currentContainer.GetInstance(x);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });

                environment["superglue.ResolveAllInstances"] = (Func<Type, IEnumerable<object>>)(x =>
                {
                    try
                    {
                        return currentContainer.GetAllInstances(x).OfType<object>();
                    }
                    catch (Exception)
                    {
                        return Enumerable.Empty<object>();
                    }
                });

                environment["superglue.Container.RegisterSingleton"] = (Action<Type, object>)((type, instance) => currentContainer.Configure(y => y.For(type).Singleton().Use(instance)));
            }, "superglue.StructureMap.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            applicationData.GetContainer().Dispose();
        }
    }
}