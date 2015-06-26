using System.Threading.Tasks;

namespace SuperGlue.EventStore.Subscribers
{
    public interface ISubscribeTo<in TEvent>
    {
        Task Handle(TEvent evnt);
    }
}