using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.HttpClient
{
    public class SetupHttpClientConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.HttpClientSetup", environment =>
            {
                environment.RegisterTransient(typeof(IHttpClient), typeof(DefaultHttpClient));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}