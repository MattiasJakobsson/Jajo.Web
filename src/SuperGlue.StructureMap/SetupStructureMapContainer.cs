using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using StructureMap;
using StructureMap.Pipeline;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.StructureMap
{
    public class SetupStructureMapContainer : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            var container = new Container();

            yield return new ConfigurationSetupResult("superglue.StructureMap.ContainerSetup", x =>
            {
                var assemblies = x.GetAssemblies();

                x[StructuremapEnvironmentExtensions.StructuremapEnvironmentKeys.ContainerKey] = container;

                x.AlterSettings<IocConfiguration>(y => y.RegisterServicesWith(z => RegisterServices(z.ToList(), container, assemblies)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }

        private IResolveServices RegisterServices(ICollection<IServiceRegistration> serviceRegistrations, IContainer container, IEnumerable<Assembly> assemblies)
        {
            var serviceResolver = new StructuremapServiceResolver(container);
            
            container.Configure(x =>
            {
                x.For<IResolveServices>().Use(serviceResolver);

                foreach (var normalRegistration in serviceRegistrations.OfType<NormalRegistration>())
                {
                    var registration = x.For(normalRegistration.ServiceType).Use(normalRegistration.ImplimentationType);

                    SetLifecycle(registration, normalRegistration.Lifecycle);
                }

                foreach (var dynamicRegistration in serviceRegistrations.OfType<DynamicRegistration>())
                {
                    var registration = x.For(dynamicRegistration.ServiceType).Use(y => dynamicRegistration.FindService(dynamicRegistration.ServiceType, new ContextServiceResolver(y)));

                    SetLifecycle(registration, dynamicRegistration.Lifecycle);
                }

                foreach (var instanceRegistration in serviceRegistrations.OfType<InstanceRegistration>())
                {
                    x.For(instanceRegistration.ServiceType).Use(instanceRegistration.Instance);
                }

                x.Scan(y =>
                {
                    foreach (var assembly in assemblies)
                        y.Assembly(assembly);

                    foreach (var scanRegistration in serviceRegistrations.OfType<ScanRegistration>())
                        y.AddAllTypesOf(scanRegistration.ScanType);

                    foreach (var genericScanRegistration in serviceRegistrations.OfType<GenericInterfaceScanRegistration>())
                        y.ConnectImplementationsToTypesClosing(genericScanRegistration.ScanType);
                });
            });

            return serviceResolver;
        }

        private void SetLifecycle<T>(ExpressedInstance<T> expressedInstance, RegistrationLifecycle lifecycle)
        {
            switch (lifecycle)
            {
                case RegistrationLifecycle.Transient:
                    expressedInstance.LifecycleIs<TransientLifecycle>();
                    break;
                case RegistrationLifecycle.Singletone:
                    expressedInstance.LifecycleIs<SingletonLifecycle>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private class ContextServiceResolver : IResolveServices
        {
            private readonly IContext _context;

            public ContextServiceResolver(IContext context)
            {
                _context = context;
            }

            public object Resolve(Type service)
            {
                return _context.TryGetInstance(service);
            }

            public IEnumerable<object> ResolveAll(Type service)
            {
                return _context.GetAllInstances(service);
            }
        }
    }
}