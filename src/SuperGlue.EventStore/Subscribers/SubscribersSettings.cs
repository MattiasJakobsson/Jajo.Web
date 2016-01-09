using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.EventStore.Subscribers
{
    public class SubscribersSettings
    {
        private readonly ICollection<Tuple<string, bool>> _streams = new List<Tuple<string, bool>>();
        private string _serviceName;
        private Func<string, string, string> _getPersistentSubscriptionGroupNameFor;

        public SubscribersSettings ServiceName(string name)
        {
            _serviceName = name;

            return this;
        }

        public SubscribersSettings SubscribeToStream(string stream, bool liveOnly = false)
        {
            _streams.Add(new Tuple<string, bool>(stream, liveOnly));

            return this;
        }

        public SubscribersSettings FindPersistentSubscriptionNameUsing(Func<string, string, string> func)
        {
            _getPersistentSubscriptionGroupNameFor = func;

            return this;
        }

        public string GetName()
        {
            return _serviceName;
        }

        public IReadOnlyCollection<Tuple<string, bool>> GetSubscribedStreams()
        {
            return new ReadOnlyCollection<Tuple<string, bool>>(_streams.ToList());
        }

        public string GetPersistentSubscriptionGroupNameFor(string stream)
        {
            return _getPersistentSubscriptionGroupNameFor(GetName(), stream);
        }
    }
}