using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.FeatureToggler
{
    public class DefaultFeatureEnabledChecker : ICheckIfFeatureIsEnabled
    {
        public bool IsEnabled<TFeature>(IDictionary<string, object> environment) where TFeature : IFeature
        {
            return IsEnabled(typeof (TFeature), environment);
        }

        public bool IsEnabled(Type featureType, IDictionary<string, object> environment)
        {
            return environment
                .GetFeatureSettings()
                .GetEnabledFeatures()
                .Contains(featureType);
        }
    }
}