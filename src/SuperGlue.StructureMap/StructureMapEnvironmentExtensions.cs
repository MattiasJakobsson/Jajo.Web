using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using SuperGlue.Configuration;

namespace SuperGlue.StructureMap
{
    public static class StructureMapEnvironmentExtensions
    {
        public class StructureMapConstants
        {
            public const string Container = "superglue.StructureMap.Container";
        }

        public static IContainer GetContainer(this IDictionary<string, object> environment)
        {
            return environment.Get<IContainer>(StructureMapConstants.Container);
        }

        internal static void SetupContainerInEnvironment(this IDictionary<string, object> environment, IContainer currentContainer)
        {
            var assemblies = environment.GetAssemblies();

            currentContainer.Configure(x => x.For<IDictionary<string, object>>().Use(environment));

            environment[ResolveExtensions.ContainerConstants.ResolveInstance] = (Func<Type, object>)(x =>
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

            environment[ResolveExtensions.ContainerConstants.ResolveAllInstances] = (Func<Type, IEnumerable<object>>)(x =>
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

            environment[ResolveExtensions.ContainerConstants.RegisterTransient] = (Action<Type, Type>)((serviceType, implimentationType) => currentContainer.Configure(y => y.For(serviceType).Use(implimentationType)));
            environment[ResolveExtensions.ContainerConstants.RegisterTransientFromFunc] = (Action<Type, Func<IDictionary<string, object>, object>>)((serviceType, getService) => currentContainer.Configure(x => x.For(serviceType).Use(y => getService(y.GetInstance<IDictionary<string, object>>()))));
            environment[ResolveExtensions.ContainerConstants.RegisterSingleton] = (Action<Type, object>)((type, instance) => currentContainer.Configure(y => y.For(type).Singleton().Use(instance)));
            environment[ResolveExtensions.ContainerConstants.RegisterSingletonType] = (Action<Type, Type>)((type, implimentationType) => currentContainer.Configure(y => y.For(type).Singleton().Use(implimentationType)));
            environment[ResolveExtensions.ContainerConstants.RegisterSingletonFromFunc] = (Action<Type, Func<IDictionary<string, object>, object>>)((serviceType, getService) => currentContainer.Configure(x => x.For(serviceType).Singleton().Use(y => getService(y.GetInstance<IDictionary<string, object>>()))));
            environment[ResolveExtensions.ContainerConstants.RegisterAllClosing] = (Action<Type>)(type => currentContainer.Configure(y => y.Scan(z =>
            {
                foreach (var assembly in assemblies)
                    z.Assembly(assembly);

                z.ConnectImplementationsToTypesClosing(type);
            })));
            environment[ResolveExtensions.ContainerConstants.RegisterAll] = (Action<Type>)(type => currentContainer.Configure(y => y.Scan(z =>
            {
                foreach (var assembly in assemblies)
                    z.Assembly(assembly);

                z.AddAllTypesOf(type);
            })));
        }
    }
}