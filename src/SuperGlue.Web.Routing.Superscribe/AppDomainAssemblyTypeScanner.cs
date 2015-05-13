using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.Routing.Superscribe
{
    public static class AppDomainAssemblyTypeScanner
    {
        public static IEnumerable<MethodInfo> GetMethodsInAssemblies(IEnumerable<Assembly> assembliesToSearch)
        {
            return assembliesToSearch.SelectMany(x => x.GetTypes()).SelectMany(x => x.GetMethods());
        }
    }
}