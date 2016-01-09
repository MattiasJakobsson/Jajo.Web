using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Notifications
{
    public class ConfigureNotifications : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Notifications.Configured", settings =>
            {
                settings.RegisterAll(typeof(IRecieveNotifications));

                settings[NotificationExtensions.NotificationConstants.NotifyError] = (Func<string, string, IDictionary<string, object>, Exception, Task>)(async (from, message, environment, exception) =>
                {
                    var notifiers = environment.ResolveAll<IRecieveNotifications>();

                    await Task.WhenAll(notifiers.Select(x => x.ErrorNotification(from, message, environment, exception)));
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}
