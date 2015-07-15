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

                return Task.CompletedTask;
            }, "superglue.ContainerSetup", configureAction: configuration =>
            {
                configuration.Settings[PartialEnvironmentExtensions.PartialConstants.PartialSettings] = configuration.WithSettings<PartialSettings>();

                return Task.CompletedTask;
            });
        }
    }
}