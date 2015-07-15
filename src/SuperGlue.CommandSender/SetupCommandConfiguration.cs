using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.CommandSender
{
    public class SetupCommandConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.CommandSenderSetup", environment =>
            {
                environment.RegisterAllClosing(typeof(IHandleCommand<>));
                environment.RegisterTransient(typeof(ISendCommand), typeof(DefaultCommandSender));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}