using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var parentContainer = environment.GetContainer();

            using (var container = parentContainer.GetNestedContainer())
            {
                environment.SetupContainerInEnvironment(container);

                await _next(environment).ConfigureAwait(false);
            }
        }
    }
}