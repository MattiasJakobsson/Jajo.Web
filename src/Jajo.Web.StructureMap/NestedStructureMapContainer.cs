using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructureMap;

namespace Jajo.Web.StructureMap
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
                var currentContainer = container;

                environment["jajo.ResolveInstance"] = (Func<Type, object>) (x =>
                {
                    try
                    {
                        return currentContainer.GetInstance(x);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });

                environment["jajo.ResolveAllInstances"] = (Func<Type, IEnumerable<object>>)(x =>
                {
                    try
                    {
                        return currentContainer.GetAllInstances(x).OfType<object>();
                    }
                    catch (Exception)
                    {
                        return Enumerable.Empty<object>();
                    }
                });

                await _next(environment);
            }
        }
    }
}