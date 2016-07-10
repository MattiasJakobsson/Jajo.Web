using System.Collections.Generic;
using StructureMap;

namespace SuperGlue.StructureMap
{
    public static class StructuremapEnvironmentExtensions
    {
        public class StructuremapEnvironmentKeys
        {
            public const string ContainerKey = "superglue.StructuremapContainer";
        }

        public static IContainer GetStructuremapContainer(this IDictionary<string, object> environment)
        {
            return environment.Get<IContainer>(StructuremapEnvironmentKeys.ContainerKey);
        }
    }
}