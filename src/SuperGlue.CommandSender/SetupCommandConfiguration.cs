using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.CommandSender
{
    public class SetupCommandConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.CommandSenderSetup", environment =>
            {
                environment.RegisterAllClosing(typeof(IHandleCommand<>));
                environment.RegisterTransient(typeof(ISendCommand), typeof(DefaultCommandSender));
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }

        public void Configure(SettingsConfiguration configuration)
        {
            
        }
    }
}