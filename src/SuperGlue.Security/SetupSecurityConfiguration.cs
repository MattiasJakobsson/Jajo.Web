using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SuperGlue.Configuration;
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
                environment.RegisterAll(typeof(IAuthenticationTokenSource));
                environment.RegisterAll(typeof(IFindRequiredAuthorizationInformationFromRequest));
                environment.RegisterAllClosing(typeof(IValidateAuthorizationInformation<>));

                environment.RegisterTransient(typeof(IHasher), (x, y) => new DefaultHasher(y.GetHasherSettings()));
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.CompletedTask;
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            configuration.Settings[SecurityEnvironmentExtensions.SecurityConstants.HasherSettings] = configuration.WithSettings<HasherSettings>();

            return Task.CompletedTask;
        }
    }
}