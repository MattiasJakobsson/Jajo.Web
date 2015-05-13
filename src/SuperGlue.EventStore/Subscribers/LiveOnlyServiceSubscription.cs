using System;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.Subscribers
{
    public class LiveOnlyServiceSubscription : IServiceSubscription
    {
        private readonly IDisposable _observableSubscription;
        private readonly EventStoreSubscription _eventStoreSubscription;

        public LiveOnlyServiceSubscription(IDisposable observableSubscription, EventStoreSubscription eventStoreSubscription)
        {
            _observableSubscription = observableSubscription;
            _eventStoreSubscription = eventStoreSubscription;
        }

        public void Close()
        {
            _eventStoreSubscription.Close();
            _observableSubscription.Dispose();
        }
    }
}