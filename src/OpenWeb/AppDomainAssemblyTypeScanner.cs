using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenWeb
{
    public static class AppDomainAssemblyTypeScanner
    {
        public static IEnumerable<MethodInfo> GetMethodsInAssemblies(IEnumerable<Assembly> assembliesToSearch, Func<MethodInfo, bool> predicate)
        {
            return assembliesToSearch.SelectMany(x => x.GetTypes()).SelectMany(x => x.GetMethods().Where(predicate));
        }
    }
}