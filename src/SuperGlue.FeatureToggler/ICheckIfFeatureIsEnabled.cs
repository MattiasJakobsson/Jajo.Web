using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.FeatureToggler
{
    public interface ICheckIfFeatureIsEnabled
    {
        Task<bool> IsEnabled<TFeature>(IDictionary<string, object> environment) where TFeature : IFeature;
        Task<bool> IsEnabled(Type featureType, IDictionary<string, object> environment);
    }
}