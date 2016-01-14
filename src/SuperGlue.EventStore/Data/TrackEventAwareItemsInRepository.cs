using SuperGlue.EventTracking;

namespace SuperGlue.EventStore.Data
{
    public class TrackEventAwareItemsInRepository : IAmInterestedInEventAwareItems
    {
        private readonly IRepository _repository;

        public TrackEventAwareItemsInRepository(IRepository repository)
        {
            _repository = repository;
        }

        public void Track(ICanApplyEvents canApplyEvents)
        {
            _repository.Attache(canApplyEvents);
        }
    }
}