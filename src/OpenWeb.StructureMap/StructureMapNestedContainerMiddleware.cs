using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StructureMap;

namespace OpenWeb.StructureMap
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StructureMapNestedContainerMiddleware
    {
        private readonly AppFunc _next;
        private readonly IContainer _container;

        public StructureMapNestedContainerMiddleware(AppFunc next, IContainer container)
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

                environment.ConfigureResolvers(x =>
                {
                    try
                    {
                        return currentContainer.GetInstance(x);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }, x =>
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