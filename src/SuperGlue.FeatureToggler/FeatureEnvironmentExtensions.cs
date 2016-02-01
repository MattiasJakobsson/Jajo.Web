using System.Collections.Generic;

namespace SuperGlue.FeatureToggler
{
    public static class FeatureEnvironmentExtensions
    {
        public static class FeatureConstants
        {
            public const string FeatureValidationFailed = "superglue.FeatureValidationFailed";
        }

        public static bool HasFeatureValidationFailed(this IDictionary<string, object> environment)
        {
            return environment.Get(FeatureConstants.FeatureValidationFailed, false);
        }
    }
}