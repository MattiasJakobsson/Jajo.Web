namespace SuperGlue.EventStore.ProcessManagers
{
    public interface IManageProcessManagerStreamEventNumbers
    {
        int? GetLastEvent(string processManager);
        void UpdateLastEvent(string processManager, int lastEvent);
    }
}