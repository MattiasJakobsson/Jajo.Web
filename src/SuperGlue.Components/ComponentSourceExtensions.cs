using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Components
{
    public static class ComponentSourceExtensions
    {
        public static async Task<IComponent> FindComponent(this IEnumerable<IComponentSource> sources, string name, IDictionary<string, object> environment)
        {
            foreach (var source in sources)
            {
                var component = await source.Find(environment, name).ConfigureAwait(false);

                if (component != null)
                    return component;
            }

            return null;
        }
    }
}