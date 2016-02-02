using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Templates
{
    public static class TemplateSourceExtensions
    {
        public static async Task<string> FindTemplate(this IEnumerable<ITemplateSource> sources, string name, IDictionary<string, object> environment)
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