namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class ProcessManagerEventNumber
    {
        public string Id { get; set; }
        public int? LastEvent { get; set; }

        public static string GetId(string service, string processManager)
        {
            return string.Format("Services/{0}/ProcessManagers/{1}", service, processManager);
        }
    }
}