using System;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ProcessManagerSubscription
    {
        private readonly IDisposable _observableSubscription;
        private readonly EventStoreStreamCatchUpSubscription _eventStoreSubscription;

        public ProcessManagerSubscription(IDisposable observableSubscription, EventStoreStreamCatchUpSubscription eventStoreSubscription)
        {
            _observableSubscription = observableSubscription;
            _eventStoreSubscription = eventStoreSubscription;
        }

        public bool Close()
        {
            try
            {
                _eventStoreSubscription.Stop(TimeSpan.FromSeconds(5));
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