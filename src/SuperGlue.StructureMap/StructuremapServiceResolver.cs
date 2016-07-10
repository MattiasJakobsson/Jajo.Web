using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.StructureMap
{
    public class StructuremapServiceResolver : IResolveServices
    {
        private readonly IContainer _container;

        public StructuremapServiceResolver(IContainer container)
        {
            _container = container;
        }

        public object Resolve(Type service)
        {
            return _container.GetInstance(service);
        }

        public IEnumerable<object> ResolveAll(Type service)
        {
            return _container.GetAllInstances(service).OfType<object>();
        }

        public IContainer GetContainer()
        {
            return _container;
        }
    }
}