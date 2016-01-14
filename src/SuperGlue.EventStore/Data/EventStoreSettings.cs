using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.Data
{
    public class EventStoreSettings
    {
        private readonly ICollection<Action<ConnectionSettingsBuilder>> _settingsModifiers = new Collection<Action<ConnectionSettingsBuilder>>();

        public EventStoreSettings()
        {
            ConnectionStringName = "EventStore";
        }

        public string ConnectionStringName { get; private set; }
        public Func<IDictionary<string, object>, object, string, string, string> FindCommandStreamFor { get; private set; }

        public EventStoreSettings UseConnectionStringName(string connectionString)
        {
            ConnectionStringName = connectionString;
            return this;
        }

        public EventStoreSettings ModifySettings(Action<ConnectionSettingsBuilder> modifier)
        {
            _settingsModifiers.Add(modifier);
            return this;
        }

        public EventStoreSettings StoreCommands(Func<IDictionary<string, object>, object, string, string, string> findStream)
        {
            FindCommandStreamFor = findStream;

            return this;
        }

        public EventStoreSettings DontStoreCommands()
        {
            FindCommandStreamFor = null;

            return this;
        }

        internal Tuple<EventStoreConnectionString, IEventStoreConnection> CreateConnection()
        {
            var connectionString = new EventStoreConnectionString(ConnectionStringName);

            var connection = connectionString.CreateConnection(x =>
            {
                foreach (var modifier in _settingsModifiers)
                    modifier(x);
            });

            return new Tuple<EventStoreConnectionString, IEventStoreConnection>(connectionString, connection);
        }
    }
}