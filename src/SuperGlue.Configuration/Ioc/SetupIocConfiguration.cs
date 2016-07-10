using System.Collections.Generic;
using System.Threading.Tasks;
using TinyIoC;

namespace SuperGlue.Configuration.Ioc
{
    public class SetupIocConfiguration : ISetupConfigurations
    {
        public const string ServiceResolverKey = "superglue.ServiceReoslver";

        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ContainerSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.RegisterServicesWith(RegisterServices));

                return Task.CompletedTask;
            }, configureAction: x =>
            {
                x.Settings[ServiceResolverKey] = x.WithSettings<IocConfiguration>().RegisterContainer();

                return Task.CompletedTask;
            });
        }

        private IResolveServices RegisterServices(IEnumerable<IServiceRegistration> serviceRegistrations)
        {
            var container = new TinyIoCContainer();

            //TODO:Register services
            return new TinyIocServiceResolver(container);
        }
    }
}