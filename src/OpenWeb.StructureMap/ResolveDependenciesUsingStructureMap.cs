using System;
using System.Collections.Generic;
using StructureMap;

namespace OpenWeb.StructureMap
{
    public class ResolveDependenciesUsingStructureMap : IResolveDependencies
    {
        private readonly IContainer _container;

        public ResolveDependenciesUsingStructureMap(IContainer container)
        {
            _container = container;
            container.Configure(x => x.For<IResolveDependencies>().Use(this));
        }

        public TService Resolve<TService>()
        {
            return _container.GetInstance<TService>();
        }

        public object Resolve(Type serviceType)
        {
            return _container.GetInstance(serviceType);
        }

        public IEnumerable<TService> ResolveAll<TService>()
        {
            return _container.GetAllInstances<TService>();
        }
    }
}