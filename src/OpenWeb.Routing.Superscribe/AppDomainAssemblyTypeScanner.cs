using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenWeb.Routing.Superscribe
{
    public static class AppDomainAssemblyTypeScanner
    {
        public static IEnumerable<MethodInfo> GetMethodsInAssemblies(IEnumerable<Assembly> assembliesToSearch)
        {
            return assembliesToSearch.SelectMany(x => x.GetTypes()).SelectMany(x => x.GetMethods());
        }
    }
}