using System;

namespace SuperGlue.Configuration.Ioc
{
    public static class IocConfigurationExtensions
    {
        public static IocConfiguration Register(this IocConfiguration iocConfiguration, Type serviceType, Type implimentationType,
            RegistrationLifecycle lifecycle = RegistrationLifecycle.Transient)
        {
            return iocConfiguration.Register(new NormalRegistration(serviceType, implimentationType, lifecycle));
        }

        public static IocConfiguration Register(this IocConfiguration iocConfiguration, Type serviceType,
            Func<Type, IResolveServices, object> findService,
            RegistrationLifecycle lifecycle = RegistrationLifecycle.Transient)
        {
            return iocConfiguration.Register(new DynamicRegistration(serviceType, findService, lifecycle));
        }

        public static IocConfiguration Register(this IocConfiguration iocConfiguration, Type serviceType,
            object instance)
        {
            return iocConfiguration.Register(new InstanceRegistration(instance, serviceType));
        }

        public static IocConfiguration Scan(this IocConfiguration iocConfiguration, Type serviceType)
        {
            return iocConfiguration.Register(new ScanRegistration(serviceType));
        }

        public static IocConfiguration ScanOpenType(this IocConfiguration iocConfiguration, Type serviceType)
        {
            return iocConfiguration.Register(new GenericInterfaceScanRegistration(serviceType));
        }
    }
}