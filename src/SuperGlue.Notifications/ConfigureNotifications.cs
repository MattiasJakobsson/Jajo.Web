using System;
using System.Collections.Generic;
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

                    foreach (var notifier in notifiers)
                    {
                        try
                        {
                            await notifier.ErrorNotification(@from, message, environment, exception).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            environment.Log(ex, "Failed to notify reciever: {0}", LogLevel.Error, notifier.GetType().Name);
                        }
                    }
                });

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}
