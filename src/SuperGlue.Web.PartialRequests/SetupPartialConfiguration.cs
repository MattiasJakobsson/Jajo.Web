using System.Collections.Generic;
using System.Reflection;
using SuperGlue.Configuration;

namespace SuperGlue.Web.PartialRequests
{
    public class SetupPartialConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
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
            });
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }

        public void Configure(SettingsConfiguration configuration)
        {
            configuration.Environment[PartialEnvironmentExtensions.PartialConstants.PartialSettings] = configuration.WithSettings<PartialSettings>();
        }
    }
}