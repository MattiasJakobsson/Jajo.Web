using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.Subscribers
{
    public interface ISubscriberInstaller
    {
        Task InstallConsumerGroupFor(string stream, Func<PersistentSubscriptionSettingsBuilder, PersistentSubscriptionSettingsBuilder> alterSettings = null);
    }
}