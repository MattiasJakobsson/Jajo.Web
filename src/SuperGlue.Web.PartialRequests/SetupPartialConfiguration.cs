using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.PartialRequests
{
    public class SetupPartialConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.PartialsSetup", environment =>
            {
                environment.AlterSettings<PartialSettings>(x =>
                {
                    x.CheckIfRequestIsPartialUsing(y =>
                    {
                        var methodEndpoint = y as MethodInfo;

                        if (methodEndpoint == null)
                            return false;

                        var endpointClass = methodEndpoint.DeclaringType;

                        return endpointClass != null && endpointClass.Name.Contains("Partial");
                    });
                });
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() =>
            {
                configuration.Settings[PartialEnvironmentExtensions.PartialConstants.PartialSettings] = configuration.WithSettings<PartialSettings>();
            });
        }
    }
}