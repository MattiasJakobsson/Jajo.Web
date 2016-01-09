using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.Configuration;
using SuperGlue.EventStore.Data;

namespace SuperGlue.EventStore.Subscribers
{
    public class DefaultSubscriberInstaller : ISubscriberInstaller
    {
        private readonly EventStoreConnectionString _eventStoreConnectionString;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IDictionary<string, object> _environment;

        public DefaultSubscriberInstaller(EventStoreConnectionString eventStoreConnectionString, IEventStoreConnection eventStoreConnection, IDictionary<string, object> environment)
        {
            _eventStoreConnectionString = eventStoreConnectionString;
            _eventStoreConnection = eventStoreConnection;
            _environment = environment;
        }

        public async Task InstallConsumerGroupFor(string stream, Func<PersistentSubscriptionSettingsBuilder, PersistentSubscriptionSettingsBuilder> alterSettings = null)
        {
            var settings = PersistentSubscriptionSettings.Create().PreferRoundRobin().ResolveLinkTos();

            alterSettings = alterSettings ?? (x => x);

            settings = alterSettings(settings);

            var subscriberSettings = _environment.GetSettings<SubscribersSettings>();
            var groupName = subscriberSettings.GetPersistentSubscriptionGroupNameFor(stream);

            try
            {
                await _eventStoreConnection.CreatePersistentSubscriptionAsync(stream, groupName, settings, _eventStoreConnectionString.GetUserCredentials());
            }
            catch (Exception ex)
            {
                _environment.Log(ex, "Failed to create consumer group for stream: {0}", LogLevel.Error, stream);
            }
        }
    }
}