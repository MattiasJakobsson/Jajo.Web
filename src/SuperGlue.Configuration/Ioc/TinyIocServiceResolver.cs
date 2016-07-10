using System;
using System.Collections.Generic;
using TinyIoC;

namespace SuperGlue.Configuration.Ioc
{
    public class TinyIocServiceResolver : IResolveServices
    {
        private readonly TinyIoCContainer _container;

        public TinyIocServiceResolver(TinyIoCContainer container)
        {
            _container = container;
        }

        public object Resolve(Type service)
        {
            return _container.Resolve(service);
        }

        public IEnumerable<object> ResolveAll(Type service)
        {
            return _container.ResolveAll(service);
        }
    }
}