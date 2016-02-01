using System.Collections.Generic;

namespace SuperGlue.FeatureToggler
{
    public interface IBelongToFeatures
    {
        IEnumerable<string> GetFeatures();
    }
}