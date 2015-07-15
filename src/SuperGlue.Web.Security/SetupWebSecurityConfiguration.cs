using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Security
{
    public class SetupWebSecurityConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.WebSecuritySetup", environment =>
            {
                environment.RegisterTransient(typeof(IEncryptionService), typeof(DefaultEncryptionService));
                environment.RegisterTransient(typeof(IHandleEncryptedCookies), typeof(DefaultCookieEncryptionHandler));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}