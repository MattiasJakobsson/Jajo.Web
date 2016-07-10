using System;

namespace SuperGlue.Configuration.Ioc
{
    public class NormalRegistration : IServiceRegistration
    {
        public NormalRegistration(Type serviceType, Type implimentationType, RegistrationLifecycle lifecycle = RegistrationLifecycle.Transient)
        {
            ServiceType = serviceType;
            ImplimentationType = implimentationType;
            Lifecycle = lifecycle;
        }

        public Type ServiceType { get; private set; }
        public Type ImplimentationType { get; private set; }
        public RegistrationLifecycle Lifecycle { get; private set; }
    }
}