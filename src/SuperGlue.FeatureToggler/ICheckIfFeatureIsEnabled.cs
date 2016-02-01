using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.FeatureToggler
{
    public interface ICheckIfFeatureIsEnabled
    {
        Task<bool> IsEnabled(string feature, IDictionary<string, object> environment);
    }
}