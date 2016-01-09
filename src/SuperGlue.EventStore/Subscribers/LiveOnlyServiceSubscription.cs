using System;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.Subscribers
{
    public class LiveOnlyServiceSubscription : IServiceSubscription
    {
        private readonly EventStoreSubscription _eventStoreSubscription;

        public LiveOnlyServiceSubscription(EventStoreSubscription eventStoreSubscription)
        {
            _eventStoreSubscription = eventStoreSubscription;
        }

        public bool Close()
        {
            try
            {
                _eventStoreSubscription.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}