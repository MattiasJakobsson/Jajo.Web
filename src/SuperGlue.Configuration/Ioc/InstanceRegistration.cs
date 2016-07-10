using System;

namespace SuperGlue.Configuration.Ioc
{
    public class InstanceRegistration : IServiceRegistration
    {
        public InstanceRegistration(object instance, Type serviceType = null)
        {
            Instance = instance;
            ServiceType = serviceType ?? instance.GetType();
        }

        public object Instance { get; private set; }
        public Type ServiceType { get; private set; }
    }
}