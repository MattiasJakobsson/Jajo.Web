using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StructureMap;

namespace SuperGlue.StructureMap
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class NestedStructureMapContainer
    {
        private readonly AppFunc _next;
        private readonly IContainer _container;

        public NestedStructureMapContainer(AppFunc next, IContainer container)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (container == null)
                throw new ArgumentNullException("container");

            _next = next;
            _container = container;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            using (var container = _container.GetNestedContainer())
            {
                environment.SetupContainerInEnvironment(container);

                await _next(environment);
            }
        }
    }
}