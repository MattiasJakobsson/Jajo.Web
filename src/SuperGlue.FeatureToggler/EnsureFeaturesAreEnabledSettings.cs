using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.FeatureToggler
{
    public class EnsureFeaturesAreEnabledSettings
    {
        public EnsureFeaturesAreEnabledSettings(Func<IDictionary<string, object>, Task<IEnumerable<IBelongToFeatures>>> getInputsToCheck)
        {
            GetInputsToCheck = getInputsToCheck;
        }

        public Func<IDictionary<string, object>, Task<IEnumerable<IBelongToFeatures>>> GetInputsToCheck { get; }
    }
}