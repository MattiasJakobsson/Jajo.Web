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

                if (!string.IsNullOrEmpty(registrationApi?.ConnectionString))
                {
                    var connectionString = new RegistrationConnectionString(registrationApi.ConnectionString);

                    environment.AlterSettings<ApiDiscoverySettings>(x => x.RegisterAt(new ApiDefinition(connectionString.Name, connectionString.Location, connectionString.Accepts)));
                }

                environment.SubscribeTo(ConfigurationEvents.AfterApplicationStart, async x =>
                {
                    var sources = x.ResolveAll<IApiSource>();

                    var finders = sources.Select(y => y.Find(x));

                    var definitions = (await Task.WhenAll(finders).ConfigureAwait(false)).SelectMany(y => y.ToList()).ToArray();

                    if (definitions.Any())
                        await environment.Resolve<IApiRegistry>().Register(x, definitions).ConfigureAwait(false);
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}