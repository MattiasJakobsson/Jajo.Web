using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SuperGlue.FeatureToggler
{
    public class FeatureSettings
    {
        private readonly IList<Type> _enabledFeatures = new List<Type>();

        public FeatureSettings EnabledFeature<TFeature>() where TFeature : IFeature
        {
            return EnabledFeature(typeof(TFeature));
        }

        public FeatureSettings DisableFeature<TFeature>() where TFeature : IFeature
        {
            return DisableFeature(typeof(TFeature));
        }

        public FeatureSettings EnabledFeature(Type featureType)
        {
            if (!_enabledFeatures.Contains(featureType))
                _enabledFeatures.Add(featureType);

            return this;
        }

        public FeatureSettings DisableFeature(Type featureType)
        {
            if (_enabledFeatures.Contains(featureType))
                _enabledFeatures.Remove(featureType);

            return this;
        }

        internal IReadOnlyCollection<Type> GetEnabledFeatures()
        {
            return new ReadOnlyCollection<Type>(_enabledFeatures);
        }
    }
}