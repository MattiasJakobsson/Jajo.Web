using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.FeatureToggler
{
    public class DefaultFeatureEnabledChecker : ICheckIfFeatureIsEnabled
    {
        public Task<bool> IsEnabled(string feature, IDictionary<string, object> environment)
        {
            return Task.FromResult(environment
                .GetSettings<FeatureSettings>()
                .GetEnabledFeatures()
                .Contains(feature));
        }
    }
}