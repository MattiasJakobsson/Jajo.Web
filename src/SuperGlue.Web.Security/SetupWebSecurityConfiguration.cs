using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Web.Security
{
    public class SetupWebSecurityConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.WebSecuritySetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IEncryptionService), typeof(DefaultEncryptionService))
                    .Register(typeof(IHandleEncryptedCookies), typeof(DefaultCookieEncryptionHandler)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}