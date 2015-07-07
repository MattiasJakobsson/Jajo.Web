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

        public bool Close()
        {
            try
            {
                _eventStoreSubscription.Close();
                _observableSubscription.Dispose();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}