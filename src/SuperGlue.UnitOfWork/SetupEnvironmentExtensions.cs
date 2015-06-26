using System.Collections.Generic;

namespace SuperGlue.UnitOfWork
{
    public static class SetupEnvironmentExtensions
    {
        public static class SetupConstants
        {
            public const string SetupEnvironmentMode = "superglue.SetupMode";
        }

        public static SetupMode GetMode(this IDictionary<string, object> environment)
        {
            return environment.Get(SetupConstants.SetupEnvironmentMode, SetupMode.None);
        }
    }
}