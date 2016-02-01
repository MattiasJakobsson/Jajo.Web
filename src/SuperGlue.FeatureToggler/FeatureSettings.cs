using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SuperGlue.FeatureToggler
{
    public class FeatureSettings
    {
        private readonly IList<string> _enabledFeatures = new List<string>();

        public FeatureSettings EnabledFeature(string feature)
        {
            if (!_enabledFeatures.Contains(feature))
                _enabledFeatures.Add(feature);

            return this;
        }

        public FeatureSettings DisableFeature(string feature)
        {
            if (_enabledFeatures.Contains(feature))
                _enabledFeatures.Remove(feature);

            return this;
        }

        internal IReadOnlyCollection<string> GetEnabledFeatures()
        {
            return new ReadOnlyCollection<string>(_enabledFeatures);
        }
    }
}