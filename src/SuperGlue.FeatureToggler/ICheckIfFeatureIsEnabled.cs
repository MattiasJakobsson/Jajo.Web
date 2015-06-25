using System;
using System.Collections.Generic;

namespace SuperGlue.FeatureToggler
{
    public interface ICheckIfFeatureIsEnabled
    {
        bool IsEnabled<TFeature>(IDictionary<string, object> environment) where TFeature : IFeature;
        bool IsEnabled(Type featureType, IDictionary<string, object> environment);
    }
}