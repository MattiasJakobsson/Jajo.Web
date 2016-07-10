using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.HttpClient
{
    public class SetupHttpClientConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.HttpClientSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(
                    x => x.Register(typeof(IHttpClient), typeof(DefaultHttpClient))
                        .Scan(typeof(IParseContentType)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}