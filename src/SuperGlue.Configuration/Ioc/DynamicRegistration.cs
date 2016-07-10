using System;

namespace SuperGlue.Configuration.Ioc
{
    public class DynamicRegistration : IServiceRegistration
    {
        public DynamicRegistration(Type serviceType, Func<Type, IResolveServices, object> findService, RegistrationLifecycle lifecycle = RegistrationLifecycle.Transient)
        {
            ServiceType = serviceType;
            FindService = findService;
            Lifecycle = lifecycle;
        }

        public Type ServiceType { get; private set; }
        public Func<Type, IResolveServices, object> FindService { get; private set; }
        public RegistrationLifecycle Lifecycle { get; private set; }
    }
}