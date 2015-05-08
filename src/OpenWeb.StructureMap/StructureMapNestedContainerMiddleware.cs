using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenWeb.Endpoints;
using OpenWeb.ModelBinding;
using StructureMap;

namespace OpenWeb.StructureMap
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StructureMapNestedContainerMiddleware
    {
        private readonly AppFunc _next;
        private readonly IContainer _container;
        private readonly IModelBinderCollection _modelBinderCollection;

        public StructureMapNestedContainerMiddleware(AppFunc next, IContainer container, IModelBinderCollection modelBinderCollection)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (container == null)
                throw new ArgumentNullException("container");

            _next = next;
            _container = container;
            _modelBinderCollection = modelBinderCollection;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = new OpenWebContext(environment, _modelBinderCollection);

            using (var container = _container.GetNestedContainer())
            {
                context.DependencyResolver = new ResolveDependenciesUsingStructureMap(container);

                await _next(environment);   
            }
        }
    }
}