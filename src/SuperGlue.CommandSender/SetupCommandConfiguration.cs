using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.CommandSender
{
    public class SetupCommandConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.CommandSenderSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.ScanOpenType(typeof(IHandleCommand<>)).Register(typeof(ISendCommand), typeof(DefaultCommandSender)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}