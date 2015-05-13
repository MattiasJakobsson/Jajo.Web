namespace SuperGlue.EventStore
{
    public interface IEventHandler
    {
        bool Handle(object evnt);
    }
}