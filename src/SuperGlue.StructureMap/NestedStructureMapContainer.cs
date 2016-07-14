using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.StructureMap
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class NestedStructureMapContainer
    {
        private readonly AppFunc _next;

        public NestedStructureMapContainer(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var parentContainer = environment.GetStructuremapContainer();

            using (var container = parentContainer.GetNestedContainer())
            {
                var resolver = new StructuremapServiceResolver(container);

                container.Configure(x =>
                {
                    x.For<IResolveServices>().Use(resolver);
                    x.For<IDictionary<string, object>>().Use(environment);
                });

                environment[StructuremapEnvironmentExtensions.StructuremapEnvironmentKeys.ContainerKey] = container;
                environment[SetupIocConfiguration.ServiceResolverKey] = resolver;

                await _next(environment).ConfigureAwait(false);
            }
        }
    }
}