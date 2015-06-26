using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Security.Authentication;

namespace SuperGlue.Security.Authorization
{
    public class SetupAuthorizationConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.AuthorizationSetup", environment =>
            {
                environment.RegisterAll(typeof(IAuthenticationTokenSource));
                environment.RegisterAll(typeof(IFindRequiredAuthorizationInformationFromRequest));
                environment.RegisterAllClosing(typeof(IValidateAuthorizationInformation<>));
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}