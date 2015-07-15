using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public interface ISetupConfigurations
    {
        IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment);
    }
}