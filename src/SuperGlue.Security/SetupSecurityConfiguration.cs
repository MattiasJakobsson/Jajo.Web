using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;
using SuperGlue.Security.Authentication;
using SuperGlue.Security.Authorization;

namespace SuperGlue.Security
{
    public class SetupSecurityConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.SecuritySetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Scan(typeof(IAuthenticationTokenSource))
                    .Scan(typeof(IFindRequiredAuthorizationInformationFromRequest))
                    .Scan(typeof(IAuthenticationStrategy))
                    .ScanOpenType(typeof(IValidateAuthorizationInformation<>))
                    .ScanOpenType(typeof(IAuthenticationStrategy<>))
                    .Register(typeof(IHasher), (y, z) => new DefaultHasher(environment.GetSettings<HasherSettings>()))
                    .Register(typeof(IAuthenticationService), typeof(DefaultAuthenticationService)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}