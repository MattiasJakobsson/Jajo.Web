using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration.Ioc
{
    public class IocConfiguration
    {
        private readonly ICollection<IServiceRegistration> _serviceRegistrations = new List<IServiceRegistration>();
        private Func<IEnumerable<IServiceRegistration>, IResolveServices> _handleRegistrations;

        public IocConfiguration Register(IServiceRegistration registration)
        {
            _serviceRegistrations.Add(registration);

            return this;
        }

        public IocConfiguration RegisterServicesWith(Func<IEnumerable<IServiceRegistration>, IResolveServices> handleRegistrations)
        {
            _handleRegistrations = handleRegistrations;

            return this;
        }

        internal IResolveServices RegisterContainer()
        {
            return _handleRegistrations(_serviceRegistrations);
        }
    }
}