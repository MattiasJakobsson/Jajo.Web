using System.Collections.Generic;
using StructureMap;

namespace SuperGlue.Web.StructureMap
{
    public static class StructureMapEnvironmentExtensions
    {
        public static IContainer GetContainer(this IDictionary<string, object> environment)
        {
            return environment.Get<IContainer>("superglue.StructureMap.Container");
        }
    }
}