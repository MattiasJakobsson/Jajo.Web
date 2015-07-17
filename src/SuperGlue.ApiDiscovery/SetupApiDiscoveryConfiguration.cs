using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.ApiDiscovery
{
    public class SetupApiDiscoveryConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ApiDiscoverySetup", environment =>
            {
                environment.RegisterAll(typeof(IApiSource));
                environment.RegisterAll(typeof(IParseApiResponse));
                environment.RegisterTransient(typeof(IExecuteApiRequests), typeof(DefaultApiRequestExecutor));
                environment.RegisterTransient(typeof(IApiRegistry), typeof(DefaultApiRegistry));

                var registrationApi = ConfigurationManager.ConnectionStrings["Api.Register"];

                if (registrationApi != null && !string.IsNullOrEmpty(registrationApi.ConnectionString))
                {
                    var connectionString = new RegistrationConnectionString(registrationApi.ConnectionString);

                    environment.AlterSettings<ApiDiscoverySettings>(x => x.RegisterAt(new ApiDefinition(connectionString.Name, connectionString.Location, connectionString.Accepts)));
                }

                environment.SubscribeTo(ConfigurationEvents.AfterApplicationStart, async x =>
                {
                    var sources = x.ResolveAll<IApiSource>();

                    var definitions = new List<ApiDefinition>();

                    foreach (var source in sources)
                        definitions.AddRange(await source.Find(x));

                    if (definitions.Any())
                        await environment.Resolve<IApiRegistry>().Register(x, definitions.ToArray());
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}