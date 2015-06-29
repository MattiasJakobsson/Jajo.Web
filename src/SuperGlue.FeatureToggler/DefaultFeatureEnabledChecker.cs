using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.FeatureToggler
{
    public class DefaultFeatureEnabledChecker : ICheckIfFeatureIsEnabled
    {
        public Task<bool> IsEnabled<TFeature>(IDictionary<string, object> environment) where TFeature : IFeature
        {
            return IsEnabled(typeof (TFeature), environment);
        }

        public Task<bool> IsEnabled(Type featureType, IDictionary<string, object> environment)
        {
            return Task.FromResult(environment
                .GetFeatureSettings()
                .GetEnabledFeatures()
                .Contains(featureType));
        }
    }
}